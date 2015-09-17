#
# Cookbook Name:: chef-client
# Recipe:: default
#
# Copyright 2015, YOUR_COMPANY_NAME
#
# All rights reserved - Do Not Redistribute
#

# create scheduled task to run chef-client every hour
include_recipe 'chef-client::scheduledtask'
