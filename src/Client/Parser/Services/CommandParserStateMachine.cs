using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Commands.Client.Parser
{
    /// <summary>
    /// State Machine used for parsing commands.
    /// </summary>
    internal class CommandParserStateMachine
    {
        private readonly IDictionary<int, CommandParameter> validArguments = new Dictionary<int, CommandParameter>();
        private readonly IDictionary<string, CommandParameter> validOptions = new Dictionary<string, CommandParameter>();
        private readonly CommandParserOptions options;
        private readonly ICommandsClient commandsClient;
        private int currentArgIndex = 0;
        private int argumentCount = 0;
        private bool uppercaseNextParamChar = true;
        private string currentArg = string.Empty;
        private string currentOptionName = string.Empty;
        private string currentOptionValue = string.Empty;
        private CommandParserState state;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandParserStateMachine" /> class.
        /// </summary>
        /// <param name="commandsClient">Client to use for getting command parse info.</param>
        /// <param name="options">Options to use when parsing.</param>
        public CommandParserStateMachine(
            ICommandsClient commandsClient,
            CommandParserOptions options
        )
        {
            this.commandsClient = commandsClient;
            this.options = options;
        }

        /// <summary>
        /// Gets a value indicating whether the parse was successful.
        /// </summary>
        public bool Success { get; private set; } = true;

        /// <summary>
        /// Gets the result of the parsing (should not be used if Success is false).
        /// </summary>
        public Command Result { get; } = new Command();

        /// <summary>
        /// Runs the state machine.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task Run(string message, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var charEnumerator = message.GetEnumerator();

            while (charEnumerator.MoveNext() && state != CommandParserState.End)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var current = charEnumerator.Current;

                switch (state)
                {
                    case CommandParserState.Prefix: HandlePrefixState(current); break;
                    case CommandParserState.CommandName: await HandleCommandNameState(current, cancellationToken); break;
                    case CommandParserState.Arguments: HandleArgumentsState(current); break;
                    case CommandParserState.OptionName: HandleOptionNameState(current); break;
                    case CommandParserState.OptionValue: HandleOptionValueState(current); break;
                    case CommandParserState.End: default: break;
                }
            }

            if (currentArg != string.Empty)
            {
                if (argumentCount >= validArguments.Count)
                {
                    Success = false;
                }
                else
                {
                    Result.Parameters.Add(validArguments[currentArgIndex].Name, currentArg);
                }
            }

            if (currentOptionName != string.Empty)
            {
                object value = (currentOptionValue != string.Empty)
                    ? currentOptionValue
                    : true;

                Result.Parameters.Add(currentOptionName, value);
            }
        }

        /// <summary>
        /// Handles the prefix state.
        /// </summary>
        /// <param name="input">The character input for the prefix state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HandlePrefixState(char input)
        {
            if (input != options.Prefix)
            {
                Success = false;
                state = CommandParserState.End;
                return;
            }

            state = CommandParserState.CommandName;
        }

        /// <summary>
        /// Handles the command name state.
        /// </summary>
        /// <param name="input">The character input for the command name state.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task HandleCommandNameState(char input, CancellationToken cancellationToken)
        {
            if (input == ' ')
            {
                state = CommandParserState.Arguments;
                await GetCommandParserRestrictions(cancellationToken);
                return;
            }

            if (Result.Name == string.Empty && !char.IsLetterOrDigit(input))
            {
                Success = false;
                state = CommandParserState.End;
            }

            Result.Name += input;
        }

        /// <summary>
        /// Checks to see if the command name exists and stores the parser restrictions of the command if it does.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task GetCommandParserRestrictions(CancellationToken cancellationToken)
        {
            try
            {
                var requestOptions = new ClientRequestOptions { ImpersonateUserId = options.ImpersonateUserId };
                var parameters = await commandsClient.GetCommandParameters(Result.Name, requestOptions, cancellationToken);

                foreach (var parameter in parameters)
                {
                    if (parameter.ArgumentIndex == null)
                    {
                        validOptions[parameter.Name] = parameter;
                        continue;
                    }

                    validArguments[parameter.ArgumentIndex.Value] = parameter;
                }
            }
            catch (ApiException)
            {
                Success = false;
                state = CommandParserState.End;
            }
        }

        /// <summary>
        /// Handles the arguments state.
        /// </summary>
        /// <param name="input">The character input for the arguments state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HandleArgumentsState(char input)
        {
            if (input == options.ArgSeparator && (validArguments.Count == 0 || argumentCount < validArguments.Count - 1))
            {
                if (currentArg != string.Empty)
                {
                    Result.Parameters.Add(validArguments[currentArgIndex++].Name, currentArg);
                    argumentCount++;
                }

                currentArg = string.Empty;
                return;
            }

            currentArg += input;

            if (currentArg.StartsWith(options.OptionPrefix) || currentArg.EndsWith($" {options.OptionPrefix}"))
            {
                state = CommandParserState.OptionName;
                currentOptionName = string.Empty;
                currentOptionValue = string.Empty;
                return;
            }
        }

        /// <summary>
        /// Handles the option name state.
        /// </summary>
        /// <param name="input">The character input for the arguments state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HandleOptionNameState(char input)
        {
            if (currentOptionName == string.Empty && !char.IsLetterOrDigit(input))
            {
                state = CommandParserState.Arguments;
                currentOptionName = string.Empty;
                HandleArgumentsState(input);
                return;
            }

            if (input == '-')
            {
                uppercaseNextParamChar = true;
                return;
            }

            if (input == ' ')
            {
                if (!validOptions.ContainsKey(currentOptionName))
                {
                    Success = false;
                    state = CommandParserState.End;
                    return;
                }

                var arg = currentArg.Length >= options.OptionPrefix.Length
                    ? currentArg[0..(currentArg.Length - options.OptionPrefix.Length)].Trim()
                    : string.Empty;

                if (arg != string.Empty)
                {
                    Result.Parameters.Add(validArguments[currentArgIndex++].Name, arg);
                    argumentCount++;
                }

                currentArg = string.Empty;
                state = CommandParserState.OptionValue;
                uppercaseNextParamChar = true;
                return;
            }

            var normalizedInput = uppercaseNextParamChar ? char.ToUpper(input) : input;
            currentOptionName += normalizedInput;
            uppercaseNextParamChar = false;
        }

        /// <summary>
        /// Handles the option value state.
        /// </summary>
        /// <param name="input">The character input for the arguments state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HandleOptionValueState(char input)
        {
            if (input == ' ')
            {
                Result.Parameters[currentOptionName] = currentOptionValue;
                state = CommandParserState.Arguments;
                currentArg = string.Empty;
                currentOptionName = string.Empty;
                currentOptionValue = string.Empty;
                return;
            }

            currentOptionValue += input;

            if (currentOptionValue == options.OptionPrefix)
            {
                Result.Parameters[currentOptionName] = true;
                state = CommandParserState.Arguments;
                currentArg = string.Empty;
                currentOptionName = string.Empty;
                currentOptionValue = string.Empty;
                return;
            }
        }
    }
}
