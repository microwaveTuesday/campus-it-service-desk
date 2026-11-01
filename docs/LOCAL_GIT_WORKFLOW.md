# GitHub source backup

Repository: https://github.com/microwaveTuesday/campus-it-service-desk

The initial commit records the current project. It does not establish student contribution history, completed sprints or teammate reviews. Team members must make their own genuine contributions and keep the required review/sprint evidence.

After changing and testing source files, save a new backup from the project terminal:

```powershell
git add .
git commit -m "Describe your changes"
git push
```

Only committed and pushed changes are backed up on GitHub. The private repository excludes local passwords, .env, database volumes, account records, uploaded files and local trust certificates. Keep local database volumes to retain your Admin account. A new installation starts empty and registers its own Admin accounts.
