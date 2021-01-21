# Contributing to RockLib

Please take a moment to review this document in order to make the contribution
process easy and effective for everyone involved.

Following these guidelines helps to communicate that you respect the time of
the developers managing and developing this open source project. In return,
they should reciprocate that respect in addressing your issue, assessing
changes, and helping you finalize your pull requests.

As for everything else in the project, the contributions to RockLib are governed by our [Code of Conduct](CODE_OF_CONDUCT.md).


## Using the issue tracker

First things first: **Do NOT report security vulnerabilities in public issues!** Please disclose responsibly by letting [the RockLib team](mailto:RockLibSupport@quickenloans.com?subject=Security) know upfront. We will assess the issue as soon as possible on a best-effort basis and will give you an estimate for when we have a fix and release available for an eventual public disclosure.

The GitHub issue tracker is the preferred channel for [bug reports](#bugs),
[features requests](#features) and [submitting pull
requests](#pull-requests).


## Bug reports

A bug is a _demonstrable problem_ that is caused by the code in the repository.
Good bug reports are extremely helpful - thank you!

Guidelines for bug reports:

1. **Use the GitHub issue search** &mdash; check if the issue has already been
   reported.

2. **Check if the issue has been fixed** &mdash; try to reproduce it using the
   latest `master` branch in the repository.

3. **Isolate the problem** &mdash; ideally create a reduced test case.

A good bug report shouldn't leave others needing to chase you up for more
information. Please try to be as detailed as possible in your report. What is
your environment? What steps will reproduce the issue? What OS experiences the
problem? What would you expect to be the outcome? All these details will help
people to fix any potential bugs.

Example:

> Short and descriptive example bug report title
>
> A summary of the issue and the browser/OS environment in which it occurs. If
> suitable, include the steps required to reproduce the bug.
>
> 1. This is the first step
> 2. This is the second step
> 3. Further steps, etc.
>
> `<url>` - a link to the reduced test case
>
> Any other information you want to share that is relevant to the issue being
> reported. This might include the lines of code that you have identified as
> causing the bug, and potential solutions (and your opinions on their
> merits).


## Feature requests

Feature requests are welcome. But take a moment to find out whether your idea
fits with the scope and aims of the project. It's up to *you* to make a strong
case to convince the project's developers of the merits of this feature. Please
provide as much detail and context as possible.


## Pull requests

Good pull requests - patches, improvements, new features - are a fantastic
help. They should remain focused in scope and avoid containing unrelated
commits.

**Please ask first** before embarking on any significant pull request (e.g.
implementing features, refactoring code), otherwise you risk spending a lot of
time working on something that the project's developers might not want to merge
into the project.


### For Contributors

If you have never created a pull request before, welcome :tada: :smile: [Here is a great tutorial](https://egghead.io/series/how-to-contribute-to-an-open-source-project-on-github)
on how to create a pull request..

1. [Fork](http://help.github.com/fork-a-repo/) the project, clone your fork,
   and configure the remotes:

   ```bash
   # Clone your fork of the repo into the current directory
   git clone https://github.com/<your-username>/<repo-name>
   # Navigate to the newly cloned directory
   cd <repo-name>
   # Assign the original repo to a remote called "upstream"
   git remote add upstream https://github.com/RockLib/<repo-name>
   ```

2. If you cloned a while ago, get the latest changes from upstream:

   ```bash
   git checkout master
   git pull upstream master
   ```

3. Create a new topic branch (off the main project development branch) to
   contain your feature, change, or fix:

   ```bash
   git checkout -b <topic-branch-name>
   ```

4. Please follow Chris Beams' [seven rules of a great Git commit message](https://chris.beams.io/posts/git-commit/#seven-rules):

   1. [Separate subject from body with a blank line](https://chris.beams.io/posts/git-commit/#separate)
   2. [Limit the subject line to 50 characters](https://chris.beams.io/posts/git-commit/#limit-50)
   3. [Capitalize the subject line](https://chris.beams.io/posts/git-commit/#capitalize)
   4. [Do not end the subject line with a period](https://chris.beams.io/posts/git-commit/#end)
   5. [Use the imperative mood in the subject line](https://chris.beams.io/posts/git-commit/#imperative)
   6. [Wrap the body at 72 characters](https://chris.beams.io/posts/git-commit/#wrap-72)
   7. [Use the body to explain what and why vs. how](https://chris.beams.io/posts/git-commit/#why-not-how)

5. Make sure to update, or add to the tests when appropriate.

6. If you added or changed a feature, make sure to document it accordingly in
   the `README.md` file.

7. Push your topic branch up to your fork:

   ```bash
   git push origin <topic-branch-name>
   ```

8. [Open a Pull Request](https://help.github.com/articles/using-pull-requests/)
    with a clear title and description.
   

**IMPORTANT**: By submitting a patch, you agree to license your work under the
same license as that used by the project.

## Maintainers

If you have commit access, please follow this process for merging patches and cutting new releases.

### Reviewing changes

1. Check that a change is within the scope and philosophy of the component.
2. Check that a change has any necessary tests.
3. Check that a change has any necessary documentation.
4. If there is anything you don’t like, leave a comment below the respective
   lines and submit a "Request changes" review. Repeat until everything has
   been addressed.
5. If you are not sure about something, mention specific people for help in a
   comment.
6. If there is only a tiny change left before you can merge it and you think
   it’s best to fix it yourself, do so and leave a comment about it so the
   author and others will know.
7. Once everything looks good, add an "Approve" review. Don’t forget to say
   something nice 👏🐶💖✨
8. If the commit messages follow Chris Beams' [seven rules of a great Git commit
   message](https://chris.beams.io/posts/git-commit/#seven-rules):

   1. Use the "Merge pull request" button to merge the pull request.
   2. Done! You are awesome! Thanks so much for your help 🤗

9. If the commit messages _do not_ follow our conventions:

   1. Use the "Squash and merge" button to clean up the commits and merge at
      the same time: ✨🎩
   2. Add a new commit subject and body.

---

*This document is based on the [contributing](https://github.com/hoodiehq/hoodie/blob/master/CONTRIBUTING.md) document from the [Hoodie](https://github.com/hoodiehq/hoodie) project.*
