#
# Cookbook Name:: winfirewall
# Recipe:: openports
#
# Copyright (c) 2015 The Authors, All Rights Reserved.
ruleDisplayName = 'Accelitec Service Ports'

powershell_script 'Open Ports' do
  code <<-EOH
  $features = @('RemoteDesktop-UserMode-In-TCP','WINRM-HTTP-In-TCP-NoScope')

  # set windows firewall profile to default state
  set-NetFirewallProfile -all -DefaultInboundAction Block -DefaultOutboundAction Allow

  # allow remote desktop and winrm
  foreach ($feature in $features)
    {
    if (get-NetFirewallRule -name $feature -ErrorAction SilentlyContinue)
    {
    enable-NetFirewallRule $feature
    }
  }

  # create custom port exceptions
  $ruleName = 'Accelitec_Service_Ports'
  $ports = @('80-86','443','5999')
    if (get-NetFirewallRule -name $ruleName -ErrorAction SilentlyContinue)
      {
      echo "$ruleDisplayName already exists, updating."
      set-NetFirewallRule -DisplayName "#{ruleDisplayName}" -Direction Inbound -LocalPort $ports -Protocol TCP -Action Allow
      }
    else
      {
      new-NetFirewallRule -DisplayName #{ruleDisplayName} -Name $ruleName -Direction Inbound -LocalPort $ports -Protocol TCP -Action Allow
      }
  EOH
  guard_interpreter :powershell_script
  not_if "(Get-NetFirewallPortFilter | Where-Object {$_.LocalPort -like '*80-86*'})"
end
