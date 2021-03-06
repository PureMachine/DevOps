#
# Cookbook Name:: local
# Recipe:: links
#
# Copyright 2015 PureMachine
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

directory "#{node['local']['source_destination']}"

link "#{ENV['USERPROFILE']}/source" do
  to "#{node['local']['source_destination']}"
end

link "#{ENV['USERPROFILE']}/Documents/WindowsPowerShell" do
  to "#{node['local']['source_destination']}/github/puremachine/windowspowershell" 
end
