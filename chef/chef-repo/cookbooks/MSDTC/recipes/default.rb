#
# Cookbook Name:: MSDTC
# Recipe:: default
#
# Copyright 2015, YOUR_COMPANY_NAME
#
# All rights reserved - Do Not Redistribute
#
# enable MSDTC, ensure it starts on reboot

service 'MSDTC' do
  action [:enable, :start]
end

# configure MSDTC for network access and reset

powershell_script 'MSDTCconfig' do
  code <<-EOH
  Set-DtcNetworkSetting –DtcName Local –AuthenticationLevel NoAuth –InboundTransactionsEnabled 1 –OutboundTransactionsEnabled 1 –RemoteClientAccessEnabled 1 –confirm:$false
  EOH
end

service 'MSDTC' do
  action [:restart]
end
