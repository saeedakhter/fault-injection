
Add upstream remote (once)
```
git remote add upstream git@github.com:your-org/main-service.git
```

Fetch latest changes from both remotes
```
git fetch origin
git fetch upstream
```

Update main in the clone to match upstream
```
git checkout main
git reset --hard upstream/main
git push origin main --force
```

Rebase all regression-injection/* branches onto updated main
``` bash
for branch in $(git branch -r | grep 'origin/regression-injection/' | sed 's|origin/||'); do
  echo "Rebasing $branch onto main..."

  # Checkout a local version of the branch
  git checkout -B "$branch" "origin/$branch"

  # Rebase onto updated main
  if git rebase main; then
    echo "✅ Rebase succeeded for $branch"
    git push origin "$branch" --force
  else
    echo "❌ Rebase failed for $branch – manual resolution needed"
    git rebase --abort
  fi

  echo ""
done
```

TODO: GitHub Actions
- Check out the regression repo.
- Run dotnet tool.
- Report success/failure.
