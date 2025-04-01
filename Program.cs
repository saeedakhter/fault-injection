using LibGit2Sharp;

var repoPath = @"C:\path\to\your\repo";
using var repo = new Repository(repoPath);

var main = repo.Branches["main"];
var upstreamMain = repo.Branches["upstream/main"] ?? repo.Network.Remotes["upstream"].FetchRefSpecs
    .Select(r => r.Specification)
    .Where(s => s.Contains("main"))
    .FirstOrDefault();

if (upstreamMain != null)
{
    Commands.Checkout(repo, main);
    repo.Reset(ResetMode.Hard, repo.Branches["upstream/main"].Tip);
    repo.Network.Push(repo.Network.Remotes["origin"], main.CanonicalName, new PushOptions { Force = true });
}

foreach (var branch in repo.Branches.Where(b => b.FriendlyName.StartsWith("origin/regression-injection/")))
{
    var localBranch = repo.Branches[branch.FriendlyName.Replace("origin/", "")] ?? repo.CreateBranch(branch.FriendlyName.Replace("origin/", ""), branch.Tip);
    Commands.Checkout(repo, localBranch);

    try
    {
        repo.Rebase.Start(localBranch, main, null, new Identity("Rebaser", "noreply@example.com"), new RebaseOptions());
        repo.Network.Push(repo.Network.Remotes["origin"], localBranch.CanonicalName, new PushOptions { Force = true });
        Console.WriteLine($"✅ Rebased {branch.FriendlyName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Rebase failed for {branch.FriendlyName}: {ex.Message}");
        repo.Rebase.Abort();
    }
}
