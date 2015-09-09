if (get-module -list psreadline) {
  import-module psreadline  
}

if (-not (get-command chef)) {
  c:\opscode\chefdk\bin\chef.bat shell-init powershell | Invoke-Expression
}
else {
  chef shell-init powershell | Invoke-Expression
}


# just calling rspec should work, but this makes sure the output is returned consistently.
function rspec
{
  cmd /c c:\opscode\chefdk\embedded\bin\rspec.bat $args
}

# A couple of helpers for making bundler a bit easier to work with
function reset
{
    param([switch]$local)
    if (test-path ./Gemfile.lock)
    {
        rm Gemfile.lock
    }
    cls
    if ($local) {
        bundle install --local
    }
    else {
        bundle install
    }
}

function be
{
    bundle exec $args
}

# Get Posh-Git loaded and start the ssh agent.  Any keys in ~/.ssh will be picked up.
if (get-module -list posh-git) {
  import-module posh-git -force
  Start-SshAgent -Quiet
}

# I like to customize my prompt, feel free to use it or skip it.
# If you want to try it out, just uncomment it.
<#
function global:prompt { 
    $realLASTEXITCODE = $LASTEXITCODE

    # Reset color, which can be messed up by Enable-GitColors
    $Host.UI.RawUI.ForegroundColor = $GitPromptSettings.DefaultForegroundColor

    $host.UI.RawUI.WindowTitle = $pwd.ProviderPath

    Write-Host 'Chef-PS ' -NoNewLine
    Write-Host (Split-Path -leaf $pwd) -NoNewLine
    Write-VcsStatus

    $global:LASTEXITCODE = $realLASTEXITCODE
    return '> '
}
#>
