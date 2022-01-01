Template project with all the stuff I like to have in a new Unity project. Smash that "use this template" button to get started.

There's also a configuration for [git lfs](https://git-lfs.github.com/) set up. That's optional, though, since git ignores the config if you don't have lfs installed.

`init.sh` cleans up all the cruft from this template, and then commits those changes. It also creates a project directory structure to match the meta files in the repository. I recommend running it before opening the project in the editor - that way, Unity doesn't delete the .meta files associated with the directories due to those directories not existing on your machine.
