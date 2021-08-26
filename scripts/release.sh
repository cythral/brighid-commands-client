#!/bin/bash -e

function ensure_default_branch()
{
    branch=$(git branch --show-current)
    
    if [ $branch != "master" ]; then
        echo "Must be on master to perform a release.";
        exit 1;
    fi
}

function create_release()
{
    version=$(cat version.json | jq -r '.version')
    git tag v$version > /dev/null
    git push --quiet -u --no-progress origin v$version > /dev/null
    echo "Created release for v$version"
}

function bump_version()
{
    echo -n "Enter next next version (without v prefix): "
    read -r nextVersion
    git checkout -b release-prep/v$nextVersion > /dev/null

    newVersionJsonContent=$(cat version.json | jq ".version=\"$nextVersion\"")
    echo $newVersionJsonContent > version.json

    touch .github/releases/v${nextVersion}.md
    
    git add version.json .github/releases/v$nextVersion.md > /dev/null
    git commit -m "$nextVersion Release Prep" > /dev/null
}

ensure_default_branch
# create_release
bump_version
gh auth login
gh pr create
