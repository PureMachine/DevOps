#
# Cookbook Name:: winfirewall
# Recipe:: openports
#
# Copyright (c) 2015 The Authors, All Rights Reserved.
powershell_script 'Open Ports' do
  ports = ['80-86','443','5999']
  code 'Add-WindowsFeature Web-Server'
  guard_interpreter :powershell_script
  not_if "(Get-WindowsFeature -Name Web-Server).InstallState -eq 'Installed'"
end

service 'w3svc' do
  action [:enable, :start]
end

template 'c:\inetpub\wwwroot\Default.htm' do
  source 'index.html.erb'
end
