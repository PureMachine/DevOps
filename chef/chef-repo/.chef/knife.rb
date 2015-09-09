# See https://docs.chef.io/config_rb_knife.html for more information on knife configuration options

current_dir = File.dirname(__FILE__)
log_level                :info
log_location             STDOUT
node_name                "conradc"
client_key               "#{current_dir}/conradc.pem"
validation_client_name   "accelitec-validator"
validation_key           "#{current_dir}/accelitec-validator.pem"
chef_server_url          "https://api.opscode.com/organizations/accelitec"
cookbook_path            ["#{current_dir}/../cookbooks"]
