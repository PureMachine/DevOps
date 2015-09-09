using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OctopusManager.Utils
{
    public class OctopusClient : HttpClient
    {
        public OctopusClient(string octopusServerUrl, string octopusCredentials)
            : base()
        {
            this.BaseAddress = new Uri(octopusServerUrl);
            this.DefaultRequestHeaders.Add("X-Octopus-ApiKey", octopusCredentials);
        }

        public async Task<OctopusProject> GetProjectAsync(string projectId)
        {
            var projectLink = string.Format("api/projects/{0}", projectId);
            var projectDoc = await this.GetStringAsync(projectLink);

            var project = JsonConvert.DeserializeObject<OctopusProject>(projectDoc);

            return project;
        }

        public async Task<List<OctopusProjectGroup>> GetProjectGroupsAsync()
        {
            var projectGroupsLink = string.Format("api/projectgroups/all");
            var projectGroupsDoc = await this.GetStringAsync(projectGroupsLink);

            var projectGroups = JsonConvert.DeserializeObject<List<OctopusProjectGroup>>(projectGroupsDoc);

            return projectGroups;
        }

        public async Task<dynamic> GetDeploymentProcessAsync(string id)
        {
            var deploymentProcessLink = string.Format("api/deploymentprocesses/{0}", id);
            var deploymentProcesDoc = await this.GetStringAsync(deploymentProcessLink);
            var expandoObjectConverter = new ExpandoObjectConverter();
            dynamic deployementProcess = JsonConvert.DeserializeObject<ExpandoObject>(deploymentProcesDoc, expandoObjectConverter);

            return deployementProcess;
        }

        public async Task UpdateDeploymentProcessAsync(string id, dynamic deploymentProcess)
        {
            var expandoObjectConverter = new ExpandoObjectConverter();
            var deploymentProcessDoc = JsonConvert.SerializeObject(deploymentProcess, expandoObjectConverter);

            var updateLink = string.Format("api/deploymentprocesses/{0}", id);

            var response = await this.PutAsync(updateLink, new StringContent(deploymentProcessDoc));
            var responseContent = response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<OctopusProject>> GetAllProjectsAsync()
        {
            var queryLink = string.Format("api/projects/all");
            var resultDoc = await this.GetStringAsync(queryLink);

            var result = JsonConvert.DeserializeObject<List<OctopusProject>>(resultDoc);

            return result;

        }
    }

    public class OctopusProject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DeploymentProcessId { get; set; }
        public string ProjectGroupId { get; set; }
    }

    public class OctopusProjectGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}


/*
{
  "Id": "projects-35",
  "VariableSetId": "variableset-projects-35",
  "DeploymentProcessId": "deploymentprocess-projects-35",
  "IncludedLibraryVariableSetIds": [],
  "DefaultToSkipIfAlreadyInstalled": false,
  "VersioningStrategy": {
    "DonorPackageStepId": null,
    "Template": "#{Octopus.Version.LastMajor}.#{Octopus.Version.LastMinor}.#{Octopus.Version.NextPatch}"
  },
  "Name": "Everything",
  "Slug": "everything",
  "Description": "",
  "IsDisabled": false,
  "ProjectGroupId": "ProjectGroups-1",
  "LastModifiedOn": "2014-12-16T09:42:08.699+00:00",
  "LastModifiedBy": "bob",
  "Links": {
    "Self": "/api/projects/projects-35",
    "Releases": "/api/projects/projects-35/releases{/version}{?skip}",
    "Variables": "/api/variables/variableset-projects-35",
    "DeploymentProcess": "/api/deploymentprocesses/deploymentprocess-projects-35",
    "Web": "/app#/projects/projects-35",
    "Logo": "/api/projects/projects-35/logo"
  }
}
 */

/*
 {
  "Id": "deploymentprocess-projects-35",
  "ProjectId": "projects-35",
  "Steps": [
    {
      "Id": "8cd6e032-9e01-4ab3-b91c-d37582923f86",
      "Name": "Sensio Web Service",
      "RequiresPackagesToBeAcquired": false,
      "Properties": {
        "Octopus.Action.TargetRoles": "ha-sensio-web,ha-homecontrol-web,ha-smartly-web,wf-sensio-web,wf-fredrikstad-web"
      },
      "Condition": "Success",
      "Actions": [
        {
          "Id": "8a42c0bc-9f6d-4ef5-9c2d-608727252005",
          "Name": "Deploy Sensio Web Service",
          "ActionType": "Octopus.TentaclePackage",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Package.NuGetFeedId": "feeds-teamcity",
            "Octopus.Action.EnabledFeatures": "Octopus.Features.ConfigurationTransforms,Octopus.Features.ConfigurationVariables",
            "Octopus.Action.Package.AutomaticallyRunConfigurationTransformationFiles": "True",
            "Octopus.Action.Package.AutomaticallyUpdateAppSettingsAndConnectionStrings": "True",
            "Octopus.Action.Package.DownloadOnTentacle": "False",
            "Octopus.Action.Package.NuGetPackageId": "WebService"
          },
          "SensitiveProperties": {}
        },
        {
          "Id": "aab3a8bd-2dcb-4cfb-8fd5-7a5f3354a06b",
          "Name": "Create Sensio WS Application",
          "ActionType": "Octopus.Script",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Script.ScriptBody": "## --------------------------------------------------------------------------------------\n## Input\n## --------------------------------------------------------------------------------------\n\n$virtualPath = $OctopusParameters['VirtualPath']\n$physicalPath = $OctopusParameters['PhysicalPath']\n$applicationPoolName = $OctopusParameters['ApplicationPoolName']\n$parentSite = $OctopusParameters['ParentSite']\n\n## --------------------------------------------------------------------------------------\n## Helpers\n## --------------------------------------------------------------------------------------\n# Helper for validating input parameters\nfunction Validate-Parameter([string]$foo, [string[]]$validInput, $parameterName) {\n    Write-Host \"${parameterName}: $foo\"\n    if (! $foo) {\n        throw \"No value was set for $parameterName, and it cannot be empty\"\n    }\n    \n    if ($validInput) {\n        if (! $validInput -contains $foo) {\n            throw \"'$input' is not a valid input for '$parameterName'\"\n        }\n    }\n    \n}\n\n# Helper to run a block with a retry if things go wrong\n$maxFailures = 5\n$sleepBetweenFailures = Get-Random -minimum 1 -maximum 4\nfunction Execute-WithRetry([ScriptBlock] $command) {\n\t$attemptCount = 0\n\t$operationIncomplete = $true\n\n\twhile ($operationIncomplete -and $attemptCount -lt $maxFailures) {\n\t\t$attemptCount = ($attemptCount + 1)\n\n\t\tif ($attemptCount -ge 2) {\n\t\t\tWrite-Output \"Waiting for $sleepBetweenFailures seconds before retrying...\"\n\t\t\tStart-Sleep -s $sleepBetweenFailures\n\t\t\tWrite-Output \"Retrying...\"\n\t\t}\n\n\t\ttry {\n\t\t\t& $command\n\n\t\t\t$operationIncomplete = $false\n\t\t} catch [System.Exception] {\n\t\t\tif ($attemptCount -lt ($maxFailures)) {\n\t\t\t\tWrite-Output (\"Attempt $attemptCount of $maxFailures failed: \" + $_.Exception.Message)\n\t\t\t\n\t\t\t}\n\t\t\telse {\n\t\t\t    throw \"Failed to execute command\"\n\t\t\t}\n\t\t}\n\t}\n}\n\n## --------------------------------------------------------------------------------------\n## Configuration\n## --------------------------------------------------------------------------------------\nValidate-Parameter $virtualPath -parameterName \"Virtual path\"\nValidate-Parameter $physicalPath -parameterName \"Physical path\"\nValidate-Parameter $applicationPoolName -parameterName \"Application pool\"\nValidate-Parameter $parentSite -parameterName \"Parent site\"\n\nAdd-PSSnapin WebAdministration -ErrorAction SilentlyContinue\nImport-Module WebAdministration -ErrorAction SilentlyContinue\n\n\n## --------------------------------------------------------------------------------------\n## Run\n## --------------------------------------------------------------------------------------\n\nWrite-Host \"Getting web site $parentSite\"\n$site = Get-Website -name $parentSite\nif (!$site) {\n    throw \"The web site '$parentSite' does not exist. Please create the site first.\"\n}\n\n$parts = $virtualPath -split \"[/\\\\]\"\n$name = \"\"\n\nfor ($i = 0; $i -lt $parts.Length; $i++) {\n    $name = $name + \"/\" + $parts[$i]\n    $name = $name.TrimStart('/').TrimEnd('/')\n    if ($i -eq $parts.Length - 1) {\n        \n    }\n    elseif ([string]::IsNullOrEmpty($name) -eq $false -and $name -ne \"/\") {\n        Write-Host \"Ensuring parent exists: $name\"\n\n        $app = Get-WebApplication -Name $name -Site $parentSite\n\n        if (!$app) {\n            $vdir = Get-WebVirtualDirectory -Name $name -site $parentSite\n            if (!$vdir) {\n                throw \"The application or virtual directory '$name' does not exist\"\n            }\n        }\n    }\n}\n\n$existing = Get-WebApplication -site $parentSite -Name $name\n\n\nExecute-WithRetry { \n    if (!$existing) {\n        Write-Host \"Creating web application '$name'\"\n        New-WebApplication -Site $parentSite -Name $name -ApplicationPool $applicationPoolName -PhysicalPath $physicalPath\n        Write-Host \"Web application created\"\n    } else {\n        Write-Host \"The web application '$name' already exists. Updating physical path:\"\n\n        Set-ItemProperty IIS:\\Sites\\$parentSite\\$name -name physicalPath -value $physicalPath\n\n        Write-Host \"Physical path changed to: $physicalPath\"\n    }\n}",
            "Octopus.Action.Template.Id": "ActionTemplates-2",
            "Octopus.Action.Template.Version": "0",
            "VirtualPath": "ws",
            "PhysicalPath": "#{Octopus.Action[Deploy Sensio Web Service].Output.Package.InstallationDirectoryPath}",
            "ApplicationPoolName": "DefaultAppPool",
            "ParentSite": "Default Web Site"
          },
          "SensitiveProperties": {}
        }
      ],
      "SensitiveProperties": {}
    },
    {
      "Id": "73811be7-caee-4dcb-b481-bae487ccae81",
      "Name": "Portal Web Site",
      "RequiresPackagesToBeAcquired": false,
      "Properties": {
        "Octopus.Action.TargetRoles": "ha-homecontrol-web,ha-sensio-web,ha-smartly-web,wf-sensio-web,wf-fredrikstad-web"
      },
      "Condition": "Success",
      "Actions": [
        {
          "Id": "fe5c2033-2ddf-469c-9345-7145a7556f3f",
          "Name": "Deploy Portal",
          "ActionType": "Octopus.TentaclePackage",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Package.NuGetFeedId": "feeds-teamcity",
            "Octopus.Action.EnabledFeatures": "Octopus.Features.ConfigurationTransforms,Octopus.Features.ConfigurationVariables",
            "Octopus.Action.Package.AutomaticallyRunConfigurationTransformationFiles": "True",
            "Octopus.Action.Package.AutomaticallyUpdateAppSettingsAndConnectionStrings": "True",
            "Octopus.Action.Package.DownloadOnTentacle": "False",
            "Octopus.Action.Package.NuGetPackageId": "PortalWeb"
          },
          "SensitiveProperties": {}
        },
        {
          "Id": "d0a53921-e72e-44bb-a33a-2953b1ec14e5",
          "Name": "Create Portal Application",
          "ActionType": "Octopus.Script",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Script.ScriptBody": "## --------------------------------------------------------------------------------------\n## Input\n## --------------------------------------------------------------------------------------\n\n$virtualPath = $OctopusParameters['VirtualPath']\n$physicalPath = $OctopusParameters['PhysicalPath']\n$applicationPoolName = $OctopusParameters['ApplicationPoolName']\n$parentSite = $OctopusParameters['ParentSite']\n\n## --------------------------------------------------------------------------------------\n## Helpers\n## --------------------------------------------------------------------------------------\n# Helper for validating input parameters\nfunction Validate-Parameter([string]$foo, [string[]]$validInput, $parameterName) {\n    Write-Host \"${parameterName}: $foo\"\n    if (! $foo) {\n        throw \"No value was set for $parameterName, and it cannot be empty\"\n    }\n    \n    if ($validInput) {\n        if (! $validInput -contains $foo) {\n            throw \"'$input' is not a valid input for '$parameterName'\"\n        }\n    }\n    \n}\n\n# Helper to run a block with a retry if things go wrong\n$maxFailures = 5\n$sleepBetweenFailures = Get-Random -minimum 1 -maximum 4\nfunction Execute-WithRetry([ScriptBlock] $command) {\n\t$attemptCount = 0\n\t$operationIncomplete = $true\n\n\twhile ($operationIncomplete -and $attemptCount -lt $maxFailures) {\n\t\t$attemptCount = ($attemptCount + 1)\n\n\t\tif ($attemptCount -ge 2) {\n\t\t\tWrite-Output \"Waiting for $sleepBetweenFailures seconds before retrying...\"\n\t\t\tStart-Sleep -s $sleepBetweenFailures\n\t\t\tWrite-Output \"Retrying...\"\n\t\t}\n\n\t\ttry {\n\t\t\t& $command\n\n\t\t\t$operationIncomplete = $false\n\t\t} catch [System.Exception] {\n\t\t\tif ($attemptCount -lt ($maxFailures)) {\n\t\t\t\tWrite-Output (\"Attempt $attemptCount of $maxFailures failed: \" + $_.Exception.Message)\n\t\t\t\n\t\t\t}\n\t\t\telse {\n\t\t\t    throw \"Failed to execute command\"\n\t\t\t}\n\t\t}\n\t}\n}\n\n## --------------------------------------------------------------------------------------\n## Configuration\n## --------------------------------------------------------------------------------------\nValidate-Parameter $virtualPath -parameterName \"Virtual path\"\nValidate-Parameter $physicalPath -parameterName \"Physical path\"\nValidate-Parameter $applicationPoolName -parameterName \"Application pool\"\nValidate-Parameter $parentSite -parameterName \"Parent site\"\n\nAdd-PSSnapin WebAdministration -ErrorAction SilentlyContinue\nImport-Module WebAdministration -ErrorAction SilentlyContinue\n\n\n## --------------------------------------------------------------------------------------\n## Run\n## --------------------------------------------------------------------------------------\n\nWrite-Host \"Getting web site $parentSite\"\n$site = Get-Website -name $parentSite\nif (!$site) {\n    throw \"The web site '$parentSite' does not exist. Please create the site first.\"\n}\n\n$parts = $virtualPath -split \"[/\\\\]\"\n$name = \"\"\n\nfor ($i = 0; $i -lt $parts.Length; $i++) {\n    $name = $name + \"/\" + $parts[$i]\n    $name = $name.TrimStart('/').TrimEnd('/')\n    if ($i -eq $parts.Length - 1) {\n        \n    }\n    elseif ([string]::IsNullOrEmpty($name) -eq $false -and $name -ne \"/\") {\n        Write-Host \"Ensuring parent exists: $name\"\n\n        $app = Get-WebApplication -Name $name -Site $parentSite\n\n        if (!$app) {\n            $vdir = Get-WebVirtualDirectory -Name $name -site $parentSite\n            if (!$vdir) {\n                throw \"The application or virtual directory '$name' does not exist\"\n            }\n        }\n    }\n}\n\n$existing = Get-WebApplication -site $parentSite -Name $name\n\n\nExecute-WithRetry { \n    if (!$existing) {\n        Write-Host \"Creating web application '$name'\"\n        New-WebApplication -Site $parentSite -Name $name -ApplicationPool $applicationPoolName -PhysicalPath $physicalPath\n        Write-Host \"Web application created\"\n    } else {\n        Write-Host \"The web application '$name' already exists. Updating physical path:\"\n\n        Set-ItemProperty IIS:\\Sites\\$parentSite\\$name -name physicalPath -value $physicalPath\n\n        Write-Host \"Physical path changed to: $physicalPath\"\n    }\n}",
            "Octopus.Action.Template.Id": "ActionTemplates-2",
            "Octopus.Action.Template.Version": "0",
            "VirtualPath": "Portal",
            "PhysicalPath": "#{Octopus.Action[Deploy Portal].Output.Package.InstallationDirectoryPath}",
            "ApplicationPoolName": "DefaultAppPool",
            "ParentSite": "Default Web Site"
          },
          "SensitiveProperties": {}
        }
      ],
      "SensitiveProperties": {}
    },
    {
      "Id": "e879ebcd-6c72-4df0-a639-a975b250b638",
      "Name": "WelfareApp Web Site",
      "RequiresPackagesToBeAcquired": false,
      "Properties": {
        "Octopus.Action.TargetRoles": "wf-sensio-web"
      },
      "Condition": "Success",
      "Actions": [
        {
          "Id": "1ccb1e6c-6c8d-4fe9-b7dd-b632a4cad01f",
          "Name": "Deploy WelfareApp",
          "ActionType": "Octopus.TentaclePackage",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Package.NuGetFeedId": "feeds-teamcity",
            "Octopus.Action.EnabledFeatures": "Octopus.Features.ConfigurationVariables,Octopus.Features.ConfigurationTransforms",
            "Octopus.Action.Package.AutomaticallyRunConfigurationTransformationFiles": "True",
            "Octopus.Action.Package.AutomaticallyUpdateAppSettingsAndConnectionStrings": "True",
            "Octopus.Action.Package.DownloadOnTentacle": "False",
            "Octopus.Action.Package.NuGetPackageId": "WelfareAppWeb"
          },
          "SensitiveProperties": {}
        },
        {
          "Id": "224968cc-94e1-44a6-90f7-90b897ccf968",
          "Name": "Create WelfareApp Application",
          "ActionType": "Octopus.Script",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Script.ScriptBody": "## --------------------------------------------------------------------------------------\n## Input\n## --------------------------------------------------------------------------------------\n\n$virtualPath = $OctopusParameters['VirtualPath']\n$physicalPath = $OctopusParameters['PhysicalPath']\n$applicationPoolName = $OctopusParameters['ApplicationPoolName']\n$parentSite = $OctopusParameters['ParentSite']\n\n## --------------------------------------------------------------------------------------\n## Helpers\n## --------------------------------------------------------------------------------------\n# Helper for validating input parameters\nfunction Validate-Parameter([string]$foo, [string[]]$validInput, $parameterName) {\n    Write-Host \"${parameterName}: $foo\"\n    if (! $foo) {\n        throw \"No value was set for $parameterName, and it cannot be empty\"\n    }\n    \n    if ($validInput) {\n        if (! $validInput -contains $foo) {\n            throw \"'$input' is not a valid input for '$parameterName'\"\n        }\n    }\n    \n}\n\n# Helper to run a block with a retry if things go wrong\n$maxFailures = 5\n$sleepBetweenFailures = Get-Random -minimum 1 -maximum 4\nfunction Execute-WithRetry([ScriptBlock] $command) {\n\t$attemptCount = 0\n\t$operationIncomplete = $true\n\n\twhile ($operationIncomplete -and $attemptCount -lt $maxFailures) {\n\t\t$attemptCount = ($attemptCount + 1)\n\n\t\tif ($attemptCount -ge 2) {\n\t\t\tWrite-Output \"Waiting for $sleepBetweenFailures seconds before retrying...\"\n\t\t\tStart-Sleep -s $sleepBetweenFailures\n\t\t\tWrite-Output \"Retrying...\"\n\t\t}\n\n\t\ttry {\n\t\t\t& $command\n\n\t\t\t$operationIncomplete = $false\n\t\t} catch [System.Exception] {\n\t\t\tif ($attemptCount -lt ($maxFailures)) {\n\t\t\t\tWrite-Output (\"Attempt $attemptCount of $maxFailures failed: \" + $_.Exception.Message)\n\t\t\t\n\t\t\t}\n\t\t\telse {\n\t\t\t    throw \"Failed to execute command\"\n\t\t\t}\n\t\t}\n\t}\n}\n\n## --------------------------------------------------------------------------------------\n## Configuration\n## --------------------------------------------------------------------------------------\nValidate-Parameter $virtualPath -parameterName \"Virtual path\"\nValidate-Parameter $physicalPath -parameterName \"Physical path\"\nValidate-Parameter $applicationPoolName -parameterName \"Application pool\"\nValidate-Parameter $parentSite -parameterName \"Parent site\"\n\nAdd-PSSnapin WebAdministration -ErrorAction SilentlyContinue\nImport-Module WebAdministration -ErrorAction SilentlyContinue\n\n\n## --------------------------------------------------------------------------------------\n## Run\n## --------------------------------------------------------------------------------------\n\nWrite-Host \"Getting web site $parentSite\"\n$site = Get-Website -name $parentSite\nif (!$site) {\n    throw \"The web site '$parentSite' does not exist. Please create the site first.\"\n}\n\n$parts = $virtualPath -split \"[/\\\\]\"\n$name = \"\"\n\nfor ($i = 0; $i -lt $parts.Length; $i++) {\n    $name = $name + \"/\" + $parts[$i]\n    $name = $name.TrimStart('/').TrimEnd('/')\n    if ($i -eq $parts.Length - 1) {\n        \n    }\n    elseif ([string]::IsNullOrEmpty($name) -eq $false -and $name -ne \"/\") {\n        Write-Host \"Ensuring parent exists: $name\"\n\n        $app = Get-WebApplication -Name $name -Site $parentSite\n\n        if (!$app) {\n            $vdir = Get-WebVirtualDirectory -Name $name -site $parentSite\n            if (!$vdir) {\n                throw \"The application or virtual directory '$name' does not exist\"\n            }\n        }\n    }\n}\n\n$existing = Get-WebApplication -site $parentSite -Name $name\n\n\nExecute-WithRetry { \n    if (!$existing) {\n        Write-Host \"Creating web application '$name'\"\n        New-WebApplication -Site $parentSite -Name $name -ApplicationPool $applicationPoolName -PhysicalPath $physicalPath\n        Write-Host \"Web application created\"\n    } else {\n        Write-Host \"The web application '$name' already exists. Updating physical path:\"\n\n        Set-ItemProperty IIS:\\Sites\\$parentSite\\$name -name physicalPath -value $physicalPath\n\n        Write-Host \"Physical path changed to: $physicalPath\"\n    }\n}",
            "Octopus.Action.Template.Id": "ActionTemplates-2",
            "Octopus.Action.Template.Version": "0",
            "VirtualPath": "welfareapp",
            "PhysicalPath": "#{Octopus.Action[Deploy WelfareApp].Output.Package.InstallationDirectoryPath}",
            "ApplicationPoolName": "DefaultAppPool",
            "ParentSite": "Default Web Site"
          },
          "SensitiveProperties": {}
        }
      ],
      "SensitiveProperties": {}
    },
    {
      "Id": "7e9ab2be-53d8-4e50-bec6-dd2255e2db58",
      "Name": "WelfareAdmin Web Site",
      "RequiresPackagesToBeAcquired": false,
      "Properties": {
        "Octopus.Action.TargetRoles": "wf-sensio-web,wf-fredrikstad-web"
      },
      "Condition": "Success",
      "Actions": [
        {
          "Id": "0910f2c7-5b3f-4b67-9def-a27d1d9591e9",
          "Name": "Deploy WelfareAdmin",
          "ActionType": "Octopus.TentaclePackage",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Package.NuGetFeedId": "feeds-teamcity",
            "Octopus.Action.EnabledFeatures": "Octopus.Features.ConfigurationTransforms,Octopus.Features.ConfigurationVariables",
            "Octopus.Action.Package.AutomaticallyRunConfigurationTransformationFiles": "True",
            "Octopus.Action.Package.AutomaticallyUpdateAppSettingsAndConnectionStrings": "True",
            "Octopus.Action.Package.DownloadOnTentacle": "False",
            "Octopus.Action.Package.NuGetPackageId": "WelfareAdminWeb"
          },
          "SensitiveProperties": {}
        },
        {
          "Id": "ef57e9fc-2daa-4bbc-b375-8429fb455d15",
          "Name": "Create WelfareAdmin Application",
          "ActionType": "Octopus.Script",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Script.ScriptBody": "## --------------------------------------------------------------------------------------\n## Input\n## --------------------------------------------------------------------------------------\n\n$virtualPath = $OctopusParameters['VirtualPath']\n$physicalPath = $OctopusParameters['PhysicalPath']\n$applicationPoolName = $OctopusParameters['ApplicationPoolName']\n$parentSite = $OctopusParameters['ParentSite']\n\n## --------------------------------------------------------------------------------------\n## Helpers\n## --------------------------------------------------------------------------------------\n# Helper for validating input parameters\nfunction Validate-Parameter([string]$foo, [string[]]$validInput, $parameterName) {\n    Write-Host \"${parameterName}: $foo\"\n    if (! $foo) {\n        throw \"No value was set for $parameterName, and it cannot be empty\"\n    }\n    \n    if ($validInput) {\n        if (! $validInput -contains $foo) {\n            throw \"'$input' is not a valid input for '$parameterName'\"\n        }\n    }\n    \n}\n\n# Helper to run a block with a retry if things go wrong\n$maxFailures = 5\n$sleepBetweenFailures = Get-Random -minimum 1 -maximum 4\nfunction Execute-WithRetry([ScriptBlock] $command) {\n\t$attemptCount = 0\n\t$operationIncomplete = $true\n\n\twhile ($operationIncomplete -and $attemptCount -lt $maxFailures) {\n\t\t$attemptCount = ($attemptCount + 1)\n\n\t\tif ($attemptCount -ge 2) {\n\t\t\tWrite-Output \"Waiting for $sleepBetweenFailures seconds before retrying...\"\n\t\t\tStart-Sleep -s $sleepBetweenFailures\n\t\t\tWrite-Output \"Retrying...\"\n\t\t}\n\n\t\ttry {\n\t\t\t& $command\n\n\t\t\t$operationIncomplete = $false\n\t\t} catch [System.Exception] {\n\t\t\tif ($attemptCount -lt ($maxFailures)) {\n\t\t\t\tWrite-Output (\"Attempt $attemptCount of $maxFailures failed: \" + $_.Exception.Message)\n\t\t\t\n\t\t\t}\n\t\t\telse {\n\t\t\t    throw \"Failed to execute command\"\n\t\t\t}\n\t\t}\n\t}\n}\n\n## --------------------------------------------------------------------------------------\n## Configuration\n## --------------------------------------------------------------------------------------\nValidate-Parameter $virtualPath -parameterName \"Virtual path\"\nValidate-Parameter $physicalPath -parameterName \"Physical path\"\nValidate-Parameter $applicationPoolName -parameterName \"Application pool\"\nValidate-Parameter $parentSite -parameterName \"Parent site\"\n\nAdd-PSSnapin WebAdministration -ErrorAction SilentlyContinue\nImport-Module WebAdministration -ErrorAction SilentlyContinue\n\n\n## --------------------------------------------------------------------------------------\n## Run\n## --------------------------------------------------------------------------------------\n\nWrite-Host \"Getting web site $parentSite\"\n$site = Get-Website -name $parentSite\nif (!$site) {\n    throw \"The web site '$parentSite' does not exist. Please create the site first.\"\n}\n\n$parts = $virtualPath -split \"[/\\\\]\"\n$name = \"\"\n\nfor ($i = 0; $i -lt $parts.Length; $i++) {\n    $name = $name + \"/\" + $parts[$i]\n    $name = $name.TrimStart('/').TrimEnd('/')\n    if ($i -eq $parts.Length - 1) {\n        \n    }\n    elseif ([string]::IsNullOrEmpty($name) -eq $false -and $name -ne \"/\") {\n        Write-Host \"Ensuring parent exists: $name\"\n\n        $app = Get-WebApplication -Name $name -Site $parentSite\n\n        if (!$app) {\n            $vdir = Get-WebVirtualDirectory -Name $name -site $parentSite\n            if (!$vdir) {\n                throw \"The application or virtual directory '$name' does not exist\"\n            }\n        }\n    }\n}\n\n$existing = Get-WebApplication -site $parentSite -Name $name\n\n\nExecute-WithRetry { \n    if (!$existing) {\n        Write-Host \"Creating web application '$name'\"\n        New-WebApplication -Site $parentSite -Name $name -ApplicationPool $applicationPoolName -PhysicalPath $physicalPath\n        Write-Host \"Web application created\"\n    } else {\n        Write-Host \"The web application '$name' already exists. Updating physical path:\"\n\n        Set-ItemProperty IIS:\\Sites\\$parentSite\\$name -name physicalPath -value $physicalPath\n\n        Write-Host \"Physical path changed to: $physicalPath\"\n    }\n}",
            "Octopus.Action.Template.Id": "ActionTemplates-2",
            "Octopus.Action.Template.Version": "0",
            "VirtualPath": "welfareadmin",
            "PhysicalPath": "#{Octopus.Action[Deploy WelfareAdmin].Output.Package.InstallationDirectoryPath}",
            "ApplicationPoolName": "DefaultAppPool",
            "ParentSite": "Default Web Site"
          },
          "SensitiveProperties": {}
        }
      ],
      "SensitiveProperties": {}
    },
    {
      "Id": "8a796926-6f05-424e-93e6-5e5a228d8790",
      "Name": "HomeControl Web Service",
      "RequiresPackagesToBeAcquired": false,
      "Properties": {
        "Octopus.Action.TargetRoles": "ha-homecontrol-web"
      },
      "Condition": "Success",
      "Actions": [
        {
          "Id": "51985c36-b9e3-4cd1-8126-fa8499a5f923",
          "Name": "Deploy HomeControl",
          "ActionType": "Octopus.TentaclePackage",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Package.NuGetFeedId": "feeds-teamcity",
            "Octopus.Action.EnabledFeatures": "Octopus.Features.ConfigurationTransforms,Octopus.Features.ConfigurationVariables",
            "Octopus.Action.Package.AutomaticallyRunConfigurationTransformationFiles": "True",
            "Octopus.Action.Package.AutomaticallyUpdateAppSettingsAndConnectionStrings": "True",
            "Octopus.Action.Package.DownloadOnTentacle": "False",
            "Octopus.Action.Package.NuGetPackageId": "HomeControlWebService"
          },
          "SensitiveProperties": {}
        },
        {
          "Id": "28e75dfa-1a0d-4d6d-a68d-de1a2b3a4253",
          "Name": "Create HomeControl Application",
          "ActionType": "Octopus.Script",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Script.ScriptBody": "## --------------------------------------------------------------------------------------\n## Input\n## --------------------------------------------------------------------------------------\n\n$virtualPath = $OctopusParameters['VirtualPath']\n$physicalPath = $OctopusParameters['PhysicalPath']\n$applicationPoolName = $OctopusParameters['ApplicationPoolName']\n$parentSite = $OctopusParameters['ParentSite']\n\n## --------------------------------------------------------------------------------------\n## Helpers\n## --------------------------------------------------------------------------------------\n# Helper for validating input parameters\nfunction Validate-Parameter([string]$foo, [string[]]$validInput, $parameterName) {\n    Write-Host \"${parameterName}: $foo\"\n    if (! $foo) {\n        throw \"No value was set for $parameterName, and it cannot be empty\"\n    }\n    \n    if ($validInput) {\n        if (! $validInput -contains $foo) {\n            throw \"'$input' is not a valid input for '$parameterName'\"\n        }\n    }\n    \n}\n\n# Helper to run a block with a retry if things go wrong\n$maxFailures = 5\n$sleepBetweenFailures = Get-Random -minimum 1 -maximum 4\nfunction Execute-WithRetry([ScriptBlock] $command) {\n\t$attemptCount = 0\n\t$operationIncomplete = $true\n\n\twhile ($operationIncomplete -and $attemptCount -lt $maxFailures) {\n\t\t$attemptCount = ($attemptCount + 1)\n\n\t\tif ($attemptCount -ge 2) {\n\t\t\tWrite-Output \"Waiting for $sleepBetweenFailures seconds before retrying...\"\n\t\t\tStart-Sleep -s $sleepBetweenFailures\n\t\t\tWrite-Output \"Retrying...\"\n\t\t}\n\n\t\ttry {\n\t\t\t& $command\n\n\t\t\t$operationIncomplete = $false\n\t\t} catch [System.Exception] {\n\t\t\tif ($attemptCount -lt ($maxFailures)) {\n\t\t\t\tWrite-Output (\"Attempt $attemptCount of $maxFailures failed: \" + $_.Exception.Message)\n\t\t\t\n\t\t\t}\n\t\t\telse {\n\t\t\t    throw \"Failed to execute command\"\n\t\t\t}\n\t\t}\n\t}\n}\n\n## --------------------------------------------------------------------------------------\n## Configuration\n## --------------------------------------------------------------------------------------\nValidate-Parameter $virtualPath -parameterName \"Virtual path\"\nValidate-Parameter $physicalPath -parameterName \"Physical path\"\nValidate-Parameter $applicationPoolName -parameterName \"Application pool\"\nValidate-Parameter $parentSite -parameterName \"Parent site\"\n\nAdd-PSSnapin WebAdministration -ErrorAction SilentlyContinue\nImport-Module WebAdministration -ErrorAction SilentlyContinue\n\n\n## --------------------------------------------------------------------------------------\n## Run\n## --------------------------------------------------------------------------------------\n\nWrite-Host \"Getting web site $parentSite\"\n$site = Get-Website -name $parentSite\nif (!$site) {\n    throw \"The web site '$parentSite' does not exist. Please create the site first.\"\n}\n\n$parts = $virtualPath -split \"[/\\\\]\"\n$name = \"\"\n\nfor ($i = 0; $i -lt $parts.Length; $i++) {\n    $name = $name + \"/\" + $parts[$i]\n    $name = $name.TrimStart('/').TrimEnd('/')\n    if ($i -eq $parts.Length - 1) {\n        \n    }\n    elseif ([string]::IsNullOrEmpty($name) -eq $false -and $name -ne \"/\") {\n        Write-Host \"Ensuring parent exists: $name\"\n\n        $app = Get-WebApplication -Name $name -Site $parentSite\n\n        if (!$app) {\n            $vdir = Get-WebVirtualDirectory -Name $name -site $parentSite\n            if (!$vdir) {\n                throw \"The application or virtual directory '$name' does not exist\"\n            }\n        }\n    }\n}\n\n$existing = Get-WebApplication -site $parentSite -Name $name\n\n\nExecute-WithRetry { \n    if (!$existing) {\n        Write-Host \"Creating web application '$name'\"\n        New-WebApplication -Site $parentSite -Name $name -ApplicationPool $applicationPoolName -PhysicalPath $physicalPath\n        Write-Host \"Web application created\"\n    } else {\n        Write-Host \"The web application '$name' already exists. Updating physical path:\"\n\n        Set-ItemProperty IIS:\\Sites\\$parentSite\\$name -name physicalPath -value $physicalPath\n\n        Write-Host \"Physical path changed to: $physicalPath\"\n    }\n}",
            "Octopus.Action.Template.Id": "ActionTemplates-2",
            "Octopus.Action.Template.Version": "0",
            "VirtualPath": "homecontrol",
            "PhysicalPath": "#{Octopus.Action[Deploy HomeControl].Output.Package.InstallationDirectoryPath}",
            "ApplicationPoolName": "DefaultAppPool",
            "ParentSite": "Default Web SIte"
          },
          "SensitiveProperties": {}
        }
      ],
      "SensitiveProperties": {}
    },
    {
      "Id": "5577088e-af42-447b-8662-218f2822ae7b",
      "Name": "Sensio AppWeb",
      "RequiresPackagesToBeAcquired": false,
      "Properties": {
        "Octopus.Action.TargetRoles": "ha-homecontrol-web,ha-sensio-web,wf-fredrikstad-web"
      },
      "Condition": "Success",
      "Actions": [
        {
          "Id": "ecbcffa7-4716-446d-adda-ffc70fe3aefd",
          "Name": "Deploy Sensio AppWeb",
          "ActionType": "Octopus.TentaclePackage",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Package.NuGetFeedId": "feeds-teamcity",
            "Octopus.Action.EnabledFeatures": "Octopus.Features.ConfigurationTransforms,Octopus.Features.ConfigurationVariables",
            "Octopus.Action.Package.AutomaticallyRunConfigurationTransformationFiles": "True",
            "Octopus.Action.Package.AutomaticallyUpdateAppSettingsAndConnectionStrings": "True",
            "Octopus.Action.Package.DownloadOnTentacle": "False",
            "Octopus.Action.Package.NuGetPackageId": "SensioAppWeb"
          },
          "SensitiveProperties": {}
        },
        {
          "Id": "e0e44857-4bd9-46d0-8f97-57e03244c183",
          "Name": "Create Sensio AppWeb Application",
          "ActionType": "Octopus.Script",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Script.ScriptBody": "## --------------------------------------------------------------------------------------\n## Input\n## --------------------------------------------------------------------------------------\n\n$virtualPath = $OctopusParameters['VirtualPath']\n$physicalPath = $OctopusParameters['PhysicalPath']\n$applicationPoolName = $OctopusParameters['ApplicationPoolName']\n$parentSite = $OctopusParameters['ParentSite']\n\n## --------------------------------------------------------------------------------------\n## Helpers\n## --------------------------------------------------------------------------------------\n# Helper for validating input parameters\nfunction Validate-Parameter([string]$foo, [string[]]$validInput, $parameterName) {\n    Write-Host \"${parameterName}: $foo\"\n    if (! $foo) {\n        throw \"No value was set for $parameterName, and it cannot be empty\"\n    }\n    \n    if ($validInput) {\n        if (! $validInput -contains $foo) {\n            throw \"'$input' is not a valid input for '$parameterName'\"\n        }\n    }\n    \n}\n\n# Helper to run a block with a retry if things go wrong\n$maxFailures = 5\n$sleepBetweenFailures = Get-Random -minimum 1 -maximum 4\nfunction Execute-WithRetry([ScriptBlock] $command) {\n\t$attemptCount = 0\n\t$operationIncomplete = $true\n\n\twhile ($operationIncomplete -and $attemptCount -lt $maxFailures) {\n\t\t$attemptCount = ($attemptCount + 1)\n\n\t\tif ($attemptCount -ge 2) {\n\t\t\tWrite-Output \"Waiting for $sleepBetweenFailures seconds before retrying...\"\n\t\t\tStart-Sleep -s $sleepBetweenFailures\n\t\t\tWrite-Output \"Retrying...\"\n\t\t}\n\n\t\ttry {\n\t\t\t& $command\n\n\t\t\t$operationIncomplete = $false\n\t\t} catch [System.Exception] {\n\t\t\tif ($attemptCount -lt ($maxFailures)) {\n\t\t\t\tWrite-Output (\"Attempt $attemptCount of $maxFailures failed: \" + $_.Exception.Message)\n\t\t\t\n\t\t\t}\n\t\t\telse {\n\t\t\t    throw \"Failed to execute command\"\n\t\t\t}\n\t\t}\n\t}\n}\n\n## --------------------------------------------------------------------------------------\n## Configuration\n## --------------------------------------------------------------------------------------\nValidate-Parameter $virtualPath -parameterName \"Virtual path\"\nValidate-Parameter $physicalPath -parameterName \"Physical path\"\nValidate-Parameter $applicationPoolName -parameterName \"Application pool\"\nValidate-Parameter $parentSite -parameterName \"Parent site\"\n\nAdd-PSSnapin WebAdministration -ErrorAction SilentlyContinue\nImport-Module WebAdministration -ErrorAction SilentlyContinue\n\n\n## --------------------------------------------------------------------------------------\n## Run\n## --------------------------------------------------------------------------------------\n\nWrite-Host \"Getting web site $parentSite\"\n$site = Get-Website -name $parentSite\nif (!$site) {\n    throw \"The web site '$parentSite' does not exist. Please create the site first.\"\n}\n\n$parts = $virtualPath -split \"[/\\\\]\"\n$name = \"\"\n\nfor ($i = 0; $i -lt $parts.Length; $i++) {\n    $name = $name + \"/\" + $parts[$i]\n    $name = $name.TrimStart('/').TrimEnd('/')\n    if ($i -eq $parts.Length - 1) {\n        \n    }\n    elseif ([string]::IsNullOrEmpty($name) -eq $false -and $name -ne \"/\") {\n        Write-Host \"Ensuring parent exists: $name\"\n\n        $app = Get-WebApplication -Name $name -Site $parentSite\n\n        if (!$app) {\n            $vdir = Get-WebVirtualDirectory -Name $name -site $parentSite\n            if (!$vdir) {\n                throw \"The application or virtual directory '$name' does not exist\"\n            }\n        }\n    }\n}\n\n$existing = Get-WebApplication -site $parentSite -Name $name\n\n\nExecute-WithRetry { \n    if (!$existing) {\n        Write-Host \"Creating web application '$name'\"\n        New-WebApplication -Site $parentSite -Name $name -ApplicationPool $applicationPoolName -PhysicalPath $physicalPath\n        Write-Host \"Web application created\"\n    } else {\n        Write-Host \"The web application '$name' already exists. Updating physical path:\"\n\n        Set-ItemProperty IIS:\\Sites\\$parentSite\\$name -name physicalPath -value $physicalPath\n\n        Write-Host \"Physical path changed to: $physicalPath\"\n    }\n}",
            "Octopus.Action.Template.Id": "ActionTemplates-2",
            "Octopus.Action.Template.Version": "0",
            "VirtualPath": "sensioapp",
            "PhysicalPath": "#{Octopus.Action[Deploy Sensio AppWeb].Output.Package.InstallationDirectoryPath}",
            "ApplicationPoolName": "DefaultAppPool",
            "ParentSite": "Default Web Site"
          },
          "SensitiveProperties": {}
        }
      ],
      "SensitiveProperties": {}
    },
    {
      "Id": "4899c2dc-bfcc-4652-a29b-4d6089b0a490",
      "Name": "Smartly AppWeb",
      "RequiresPackagesToBeAcquired": false,
      "Properties": {
        "Octopus.Action.TargetRoles": "ha-smartly-web"
      },
      "Condition": "Success",
      "Actions": [
        {
          "Id": "f57bce49-1b3c-4633-9aab-a3543aa03aba",
          "Name": "Deploy Smartly AppWeb",
          "ActionType": "Octopus.TentaclePackage",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Package.NuGetFeedId": "feeds-teamcity",
            "Octopus.Action.EnabledFeatures": "Octopus.Features.ConfigurationTransforms,Octopus.Features.ConfigurationVariables",
            "Octopus.Action.Package.AutomaticallyRunConfigurationTransformationFiles": "True",
            "Octopus.Action.Package.AutomaticallyUpdateAppSettingsAndConnectionStrings": "True",
            "Octopus.Action.Package.DownloadOnTentacle": "False",
            "Octopus.Action.Package.NuGetPackageId": "SmartlyAppWeb"
          },
          "SensitiveProperties": {}
        },
        {
          "Id": "caf4bd07-54c3-48fb-b5f1-9c0c69761e56",
          "Name": "Create SmartlyAppWeb Application",
          "ActionType": "Octopus.Script",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Script.ScriptBody": "## --------------------------------------------------------------------------------------\n## Input\n## --------------------------------------------------------------------------------------\n\n$virtualPath = $OctopusParameters['VirtualPath']\n$physicalPath = $OctopusParameters['PhysicalPath']\n$applicationPoolName = $OctopusParameters['ApplicationPoolName']\n$parentSite = $OctopusParameters['ParentSite']\n\n## --------------------------------------------------------------------------------------\n## Helpers\n## --------------------------------------------------------------------------------------\n# Helper for validating input parameters\nfunction Validate-Parameter([string]$foo, [string[]]$validInput, $parameterName) {\n    Write-Host \"${parameterName}: $foo\"\n    if (! $foo) {\n        throw \"No value was set for $parameterName, and it cannot be empty\"\n    }\n    \n    if ($validInput) {\n        if (! $validInput -contains $foo) {\n            throw \"'$input' is not a valid input for '$parameterName'\"\n        }\n    }\n    \n}\n\n# Helper to run a block with a retry if things go wrong\n$maxFailures = 5\n$sleepBetweenFailures = Get-Random -minimum 1 -maximum 4\nfunction Execute-WithRetry([ScriptBlock] $command) {\n\t$attemptCount = 0\n\t$operationIncomplete = $true\n\n\twhile ($operationIncomplete -and $attemptCount -lt $maxFailures) {\n\t\t$attemptCount = ($attemptCount + 1)\n\n\t\tif ($attemptCount -ge 2) {\n\t\t\tWrite-Output \"Waiting for $sleepBetweenFailures seconds before retrying...\"\n\t\t\tStart-Sleep -s $sleepBetweenFailures\n\t\t\tWrite-Output \"Retrying...\"\n\t\t}\n\n\t\ttry {\n\t\t\t& $command\n\n\t\t\t$operationIncomplete = $false\n\t\t} catch [System.Exception] {\n\t\t\tif ($attemptCount -lt ($maxFailures)) {\n\t\t\t\tWrite-Output (\"Attempt $attemptCount of $maxFailures failed: \" + $_.Exception.Message)\n\t\t\t\n\t\t\t}\n\t\t\telse {\n\t\t\t    throw \"Failed to execute command\"\n\t\t\t}\n\t\t}\n\t}\n}\n\n## --------------------------------------------------------------------------------------\n## Configuration\n## --------------------------------------------------------------------------------------\nValidate-Parameter $virtualPath -parameterName \"Virtual path\"\nValidate-Parameter $physicalPath -parameterName \"Physical path\"\nValidate-Parameter $applicationPoolName -parameterName \"Application pool\"\nValidate-Parameter $parentSite -parameterName \"Parent site\"\n\nAdd-PSSnapin WebAdministration -ErrorAction SilentlyContinue\nImport-Module WebAdministration -ErrorAction SilentlyContinue\n\n\n## --------------------------------------------------------------------------------------\n## Run\n## --------------------------------------------------------------------------------------\n\nWrite-Host \"Getting web site $parentSite\"\n$site = Get-Website -name $parentSite\nif (!$site) {\n    throw \"The web site '$parentSite' does not exist. Please create the site first.\"\n}\n\n$parts = $virtualPath -split \"[/\\\\]\"\n$name = \"\"\n\nfor ($i = 0; $i -lt $parts.Length; $i++) {\n    $name = $name + \"/\" + $parts[$i]\n    $name = $name.TrimStart('/').TrimEnd('/')\n    if ($i -eq $parts.Length - 1) {\n        \n    }\n    elseif ([string]::IsNullOrEmpty($name) -eq $false -and $name -ne \"/\") {\n        Write-Host \"Ensuring parent exists: $name\"\n\n        $app = Get-WebApplication -Name $name -Site $parentSite\n\n        if (!$app) {\n            $vdir = Get-WebVirtualDirectory -Name $name -site $parentSite\n            if (!$vdir) {\n                throw \"The application or virtual directory '$name' does not exist\"\n            }\n        }\n    }\n}\n\n$existing = Get-WebApplication -site $parentSite -Name $name\n\n\nExecute-WithRetry { \n    if (!$existing) {\n        Write-Host \"Creating web application '$name'\"\n        New-WebApplication -Site $parentSite -Name $name -ApplicationPool $applicationPoolName -PhysicalPath $physicalPath\n        Write-Host \"Web application created\"\n    } else {\n        Write-Host \"The web application '$name' already exists. Updating physical path:\"\n\n        Set-ItemProperty IIS:\\Sites\\$parentSite\\$name -name physicalPath -value $physicalPath\n\n        Write-Host \"Physical path changed to: $physicalPath\"\n    }\n}",
            "Octopus.Action.Template.Id": "ActionTemplates-2",
            "Octopus.Action.Template.Version": "0",
            "VirtualPath": "smartlyapp",
            "PhysicalPath": "#{Octopus.Action[Deploy Smartly AppWeb].Output.Package.InstallationDirectoryPath}",
            "ApplicationPoolName": "DefaultAppPool",
            "ParentSite": "Default Web Site"
          },
          "SensitiveProperties": {}
        }
      ],
      "SensitiveProperties": {}
    },
    {
      "Id": "8685d67a-1495-4e92-9752-544dc0c8feaa",
      "Name": "GraphicService Web Service",
      "RequiresPackagesToBeAcquired": false,
      "Properties": {
        "Octopus.Action.TargetRoles": "ha-sensio-web,ha-homecontrol-web,ha-smartly-web,wf-sensio-web,wf-fredrikstad-web"
      },
      "Condition": "Success",
      "Actions": [
        {
          "Id": "4cc12b21-59f5-41e2-8f75-f3875737bf4d",
          "Name": "Deploy GraphicService",
          "ActionType": "Octopus.TentaclePackage",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Package.NuGetFeedId": "feeds-teamcity",
            "Octopus.Action.EnabledFeatures": "Octopus.Features.ConfigurationTransforms,Octopus.Features.ConfigurationVariables",
            "Octopus.Action.Package.AutomaticallyRunConfigurationTransformationFiles": "True",
            "Octopus.Action.Package.AutomaticallyUpdateAppSettingsAndConnectionStrings": "True",
            "Octopus.Action.Package.DownloadOnTentacle": "False",
            "Octopus.Action.Package.NuGetPackageId": "GraphicService"
          },
          "SensitiveProperties": {}
        },
        {
          "Id": "69354f14-552c-426b-b6a6-8f9dbf7dc577",
          "Name": "Create GraphicService Application",
          "ActionType": "Octopus.Script",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Script.ScriptBody": "## --------------------------------------------------------------------------------------\n## Input\n## --------------------------------------------------------------------------------------\n\n$virtualPath = $OctopusParameters['VirtualPath']\n$physicalPath = $OctopusParameters['PhysicalPath']\n$applicationPoolName = $OctopusParameters['ApplicationPoolName']\n$parentSite = $OctopusParameters['ParentSite']\n\n## --------------------------------------------------------------------------------------\n## Helpers\n## --------------------------------------------------------------------------------------\n# Helper for validating input parameters\nfunction Validate-Parameter([string]$foo, [string[]]$validInput, $parameterName) {\n    Write-Host \"${parameterName}: $foo\"\n    if (! $foo) {\n        throw \"No value was set for $parameterName, and it cannot be empty\"\n    }\n    \n    if ($validInput) {\n        if (! $validInput -contains $foo) {\n            throw \"'$input' is not a valid input for '$parameterName'\"\n        }\n    }\n    \n}\n\n# Helper to run a block with a retry if things go wrong\n$maxFailures = 5\n$sleepBetweenFailures = Get-Random -minimum 1 -maximum 4\nfunction Execute-WithRetry([ScriptBlock] $command) {\n\t$attemptCount = 0\n\t$operationIncomplete = $true\n\n\twhile ($operationIncomplete -and $attemptCount -lt $maxFailures) {\n\t\t$attemptCount = ($attemptCount + 1)\n\n\t\tif ($attemptCount -ge 2) {\n\t\t\tWrite-Output \"Waiting for $sleepBetweenFailures seconds before retrying...\"\n\t\t\tStart-Sleep -s $sleepBetweenFailures\n\t\t\tWrite-Output \"Retrying...\"\n\t\t}\n\n\t\ttry {\n\t\t\t& $command\n\n\t\t\t$operationIncomplete = $false\n\t\t} catch [System.Exception] {\n\t\t\tif ($attemptCount -lt ($maxFailures)) {\n\t\t\t\tWrite-Output (\"Attempt $attemptCount of $maxFailures failed: \" + $_.Exception.Message)\n\t\t\t\n\t\t\t}\n\t\t\telse {\n\t\t\t    throw \"Failed to execute command\"\n\t\t\t}\n\t\t}\n\t}\n}\n\n## --------------------------------------------------------------------------------------\n## Configuration\n## --------------------------------------------------------------------------------------\nValidate-Parameter $virtualPath -parameterName \"Virtual path\"\nValidate-Parameter $physicalPath -parameterName \"Physical path\"\nValidate-Parameter $applicationPoolName -parameterName \"Application pool\"\nValidate-Parameter $parentSite -parameterName \"Parent site\"\n\nAdd-PSSnapin WebAdministration -ErrorAction SilentlyContinue\nImport-Module WebAdministration -ErrorAction SilentlyContinue\n\n\n## --------------------------------------------------------------------------------------\n## Run\n## --------------------------------------------------------------------------------------\n\nWrite-Host \"Getting web site $parentSite\"\n$site = Get-Website -name $parentSite\nif (!$site) {\n    throw \"The web site '$parentSite' does not exist. Please create the site first.\"\n}\n\n$parts = $virtualPath -split \"[/\\\\]\"\n$name = \"\"\n\nfor ($i = 0; $i -lt $parts.Length; $i++) {\n    $name = $name + \"/\" + $parts[$i]\n    $name = $name.TrimStart('/').TrimEnd('/')\n    if ($i -eq $parts.Length - 1) {\n        \n    }\n    elseif ([string]::IsNullOrEmpty($name) -eq $false -and $name -ne \"/\") {\n        Write-Host \"Ensuring parent exists: $name\"\n\n        $app = Get-WebApplication -Name $name -Site $parentSite\n\n        if (!$app) {\n            $vdir = Get-WebVirtualDirectory -Name $name -site $parentSite\n            if (!$vdir) {\n                throw \"The application or virtual directory '$name' does not exist\"\n            }\n        }\n    }\n}\n\n$existing = Get-WebApplication -site $parentSite -Name $name\n\n\nExecute-WithRetry { \n    if (!$existing) {\n        Write-Host \"Creating web application '$name'\"\n        New-WebApplication -Site $parentSite -Name $name -ApplicationPool $applicationPoolName -PhysicalPath $physicalPath\n        Write-Host \"Web application created\"\n    } else {\n        Write-Host \"The web application '$name' already exists. Updating physical path:\"\n\n        Set-ItemProperty IIS:\\Sites\\$parentSite\\$name -name physicalPath -value $physicalPath\n\n        Write-Host \"Physical path changed to: $physicalPath\"\n    }\n}",
            "Octopus.Action.Template.Id": "ActionTemplates-2",
            "Octopus.Action.Template.Version": "0",
            "VirtualPath": "gs",
            "PhysicalPath": "#{Octopus.Action[Deploy GraphicService].Output.Package.InstallationDirectoryPath}",
            "ApplicationPoolName": "DefaultAppPool",
            "ParentSite": "Default Web Site"
          },
          "SensitiveProperties": {}
        }
      ],
      "SensitiveProperties": {}
    },
    {
      "Id": "471aff92-862b-42d6-87ff-6c49eb5d1c05",
      "Name": "Traffic Server",
      "RequiresPackagesToBeAcquired": false,
      "Properties": {
        "Octopus.Action.TargetRoles": "ha-homecontrol-traffic,ha-sensio-traffic,wf-sensio-traffic,wf-fredrikstad-traffic,ha-smartly-traffic"
      },
      "Condition": "Success",
      "Actions": [
        {
          "Id": "de90f586-5ab3-4403-ae8b-bf30765b0b0f",
          "Name": "Traffic Server",
          "ActionType": "Octopus.TentaclePackage",
          "Environments": [],
          "Properties": {
            "Octopus.Action.Package.NuGetFeedId": "feeds-teamcity",
            "Octopus.Action.EnabledFeatures": "Octopus.Features.ConfigurationVariables,Octopus.Features.ConfigurationTransforms,Octopus.Features.WindowsService",
            "Octopus.Action.Package.AutomaticallyRunConfigurationTransformationFiles": "True",
            "Octopus.Action.Package.AutomaticallyUpdateAppSettingsAndConnectionStrings": "True",
            "Octopus.Action.Package.DownloadOnTentacle": "False",
            "Octopus.Action.Package.NuGetPackageId": "SensioProxy",
            "Octopus.Action.WindowsService.CreateOrUpdateService": "True",
            "Octopus.Action.WindowsService.ServiceAccount": "",
            "Octopus.Action.WindowsService.StartMode": "auto",
            "Octopus.Action.WindowsService.ServiceName": "SensioProxy",
            "Octopus.Action.WindowsService.Description": "Sensio Traffic Server",
            "Octopus.Action.WindowsService.ExecutablePath": "bin\\SensioProxy.exe"
          },
          "SensitiveProperties": {}
        }
      ],
      "SensitiveProperties": {}
    }
  ],
  "Version": 21,
  "LastModifiedOn": "2015-01-06T15:34:33.713+00:00",
  "LastModifiedBy": "bob",
  "Links": {
    "Self": "/api/deploymentprocesses/deploymentprocess-projects-35",
    "Project": "/api/projects/projects-35",
    "Template": "/api/deploymentprocesses/deploymentprocess-projects-35/template"
  }
}*/

/*
Id = 95, Status = RanToCompletion, Method = "{null}", Result = "{\r\n  \"Id\": \"projects-70\",\r\n  \"VariableSetId\": \"variableset-projects-70\",\r\n  \"DeploymentProcessId\": \"deploymentprocess-projects-70\",\r\n  \"IncludedLibraryVariableSetIds\": [],\r\n  \"DefaultToSkipIfAlreadyInstalled\": false,\r\n  \"VersioningStrategy\": {\r\n    \"DonorPackageStepId\": null,\r\n    \"Template\": \"#{Octopus.Version.LastMajor}.#{Octopus.Version.LastMinor}.#{Octopus.Version.NextPatch}\"\r\n  },\r\n  \"Name\": \"Fredy Test\",\r\n  \"Slug\": \"fredy-test\",\r\n  \"Description\": null,\r\n  \"IsDisabled\": false,\r\n  \"ProjectGroupId\": \"ProjectGroups-1\",\r\n  \"LastModifiedOn\": \"2015-01-14T22:55:33.840+00:00\",\r\n  \"LastModifiedBy\": \"bob\",\r\n  \"Links\": {\r\n    \"Self\": \"/api/projects/projects-70\",\r\n    \"Releases\": \"/api/projects/projects-70/releases{/version}{?skip}\",\r\n    \"Variables\": \"/api/variables/variableset-projects-70\",\r\n    \"DeploymentProcess\": \"/api/deploymentprocesses/deploymentprocess-projects-70\",\r\n    \"Web\": \"/app#/projects/projects-70\",\r\n    \"Logo\": \"/api/projects/projects-70/logo\"\r\n  }\r\n}"
Id = 334, Status = RanToCompletion, Method = "{null}", Result = "{\r\n  \"Id\": \"projects-72\",\r\n  \"VariableSetId\": \"variableset-projects-72\",\r\n  \"DeploymentProcessId\": \"deploymentprocess-projects-72\",\r\n  \"IncludedLibraryVariableSetIds\": [],\r\n  \"DefaultToSkipIfAlreadyInstalled\": false,\r\n  \"VersioningStrategy\": {\r\n    \"DonorPackageStepId\": null,\r\n    \"Template\": \"#{Octopus.Version.LastMajor}.#{Octopus.Version.LastMinor}.#{Octopus.Version.NextPatch}\"\r\n  },\r\n  \"Name\": \"Fredy Test\",\r\n  \"Slug\": \"fredy-test\",\r\n  \"Description\": null,\r\n  \"IsDisabled\": false,\r\n  \"ProjectGroupId\": \"ProjectGroups-1\",\r\n  \"LastModifiedOn\": \"2015-01-14T23:14:41.264+00:00\",\r\n  \"LastModifiedBy\": \"bob\",\r\n  \"Links\": {\r\n    \"Self\": \"/api/projects/projects-72\",\r\n    \"Releases\": \"/api/projects/projects-72/releases{/version}{?skip}\",\r\n    \"Variables\": \"/api/variables/variableset-projects-72\",\r\n    \"DeploymentProcess\": \"/api/deploymentprocesses/deploymentprocess-projects-72\",\r\n    \"Web\": \"/app#/projects/projects-72\",\r\n    \"Logo\": \"/api/projects/projects-72/logo\"\r\n  }\r\n}" 
 */