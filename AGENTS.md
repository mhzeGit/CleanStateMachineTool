# CONTEXT.md Auto-Updater Rule

If a `CONTEXT.md` file exists in the project root, keep it in sync with structural file changes:

- After creating a new file → add it with a one-line description
- After deleting a file → remove its entry
- After renaming a file → update the name and description
- After changing the general purpose of a file → update its description

Do this automatically without being asked. Only trigger for structural changes (create, delete, rename, repurpose). Do not update for regular edits or minor changes inside a script.
