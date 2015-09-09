using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.Http;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OctopusManager.Utils;

namespace OctopusManager.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private string _octopusServerUrl = "http://build1";
        private string _octopusCredentials = "office\\conradc";

        private string _sourceProjectName = "DeployMM";
        private string _sourceProjectDocument;

        private DeploymentProcessStepViewModel _selectedSourceStep;

        private readonly ObservableCollection<DeploymentProcessStepViewModel> _sourceProcessStepsList = new ObservableCollection<DeploymentProcessStepViewModel>();
        private readonly ObservableCollection<SelectProjectViewModel> _targetProjectsList = new ObservableCollection<SelectProjectViewModel>();

        private readonly ICollectionView _sourceProcessStepsCollectionView;
        private readonly ICollectionView _targetProjectsCollectionView;

        private string _status = "All OK";

        private ICommand _loadSourceProjectCommand;
        private ICommand _copyStepCommand;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _sourceProcessStepsCollectionView = new ListCollectionView(_sourceProcessStepsList);
            _targetProjectsCollectionView = new ListCollectionView(_targetProjectsList);

            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }

        public string OctopusCredentials
        {
            get { return _octopusCredentials; }
            set
            {
                if (value != null)
                {
                    _octopusCredentials = value;
                    RaisePropertyChanged(() => OctopusCredentials);
                }
            }
        }

        public string SourceProjectName
        {
            get { return _sourceProjectName; }
            set
            {
                if (value != null)
                {
                    _sourceProjectName = value;
                    RaisePropertyChanged(() => SourceProjectName);
                }
            }
        }

        public string SourceProjectDocument
        {
            get { return _sourceProjectDocument; }
            set
            {
                if (value != null)
                {
                    _sourceProjectDocument = value;
                    RaisePropertyChanged(() => SourceProjectDocument);
                }
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                if (value != _status)
                {
                    _status = value;
                    RaisePropertyChanged(() => Status);
                }
            }
        }

        public ICollectionView SourceProcessSteps
        {
            get { return _sourceProcessStepsCollectionView; }
        }

        public ICollectionView TargetProjects
        {
            get { return _targetProjectsCollectionView; }
        }

        public DeploymentProcessStepViewModel SelectedSourceStep
        {
            get { return _selectedSourceStep; }
            set
            {
                if (value != _selectedSourceStep)
                {
                    _selectedSourceStep = value;
                    RaisePropertyChanged(() => SelectedSourceStep);
                }
            }
        }

        public ICommand LoadSourceProjectCommand
        {
            get
            {
                return _loadSourceProjectCommand ?? (_loadSourceProjectCommand = new RelayCommand(
                    async () =>
                    {
                        try
                        {
                            using (var client = new OctopusClient(_octopusServerUrl, _octopusCredentials))
                            {
                                try
                                {
                                    var sourceProject = await client.GetProjectAsync(_sourceProjectName);
                                    var sourceDeploymentProject = await client.GetDeploymentProcessAsync(sourceProject.DeploymentProcessId);

                                    _sourceProcessStepsList.Clear();
                                    foreach (var step in sourceDeploymentProject.Steps)
                                    {
                                        _sourceProcessStepsList.Add(new DeploymentProcessStepViewModel
                                        {
                                            Id = step.Id,
                                            Name = step.Name
                                        });
                                    }

                                    var projects = await client.GetAllProjectsAsync();

                                    _targetProjectsList.Clear();
                                    projects.ForEach(p => _targetProjectsList.Add(new SelectProjectViewModel
                                    {
                                        Id = p.Id,
                                        Name = p.Name
                                    }));

                                    Status = "Load Source Project - Success";
                                }
                                catch (HttpRequestException ex)
                                {
                                    Status = ex.Message;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Status = ex.Message;
                        }
                    }));
            }
        }

        public ICommand CopyStepCommand
        {
            get
            {
                return _copyStepCommand ?? (_copyStepCommand = new RelayCommand(
                    async () =>
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(_sourceProjectName))
                                throw new Exception("Soruce project not specified");

                            if (_selectedSourceStep == null)
                                throw new Exception("Source process step is not selected");

                            var targetProjectIdList = _targetProjectsList.Where(p => p.IsSelected).Select(p => p.Id).ToList();

                            if (!targetProjectIdList.Any())
                                throw new Exception("No target projects selected");

                            using (var client = new OctopusClient(_octopusServerUrl, _octopusCredentials))
                            {
                                try
                                {
                                    var sourceProject = await client.GetProjectAsync(_sourceProjectName);
                                    var sourceDeploymentProject = await client.GetDeploymentProcessAsync(sourceProject.DeploymentProcessId);

                                    dynamic stepToCopy = null;
                                    foreach (var step in sourceDeploymentProject.Steps)
                                    {
                                        if (step.Id == _selectedSourceStep.Id)
                                        {
                                            stepToCopy = step;
                                            break;
                                        }
                                    }

                                    if (stepToCopy == null)
                                        throw new Exception("No step to copy");


                                    foreach (var targetProjectId in targetProjectIdList)
                                    {
                                        var targetProject = await client.GetProjectAsync(targetProjectId);
                                        var targetDeploymentProcess = await client.GetDeploymentProcessAsync(targetProject.DeploymentProcessId);

                                        var stepClone = MakeExpandoClone(stepToCopy);

                                        // Make new step id
                                        stepClone.Id = Guid.NewGuid().ToString();

                                        // Don't clone roles
                                        //var stepCloneProperties = stepClone.Properties as IDictionary<string,object>;
                                        //if (stepCloneProperties != null)
                                        //    stepCloneProperties["Octopus.Action.TargetRoles"] = "";

                                        // Make new step action ids
                                        foreach (var action in stepClone.Actions)
                                        {
                                            action.Id = Guid.NewGuid().ToString();
                                        }

                                        // Add cloned step to target project deployment process
                                        targetDeploymentProcess.Steps.Add(stepClone);

                                        await client.UpdateDeploymentProcessAsync(targetProject.DeploymentProcessId, targetDeploymentProcess);
                                    }

                                    Status = "Copy Step - Success";
                                }
                                catch (HttpRequestException ex)
                                {
                                    Status = ex.Message;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Status = ex.Message;
                        }
                    }));
            }
        }

        private static ExpandoObject MakeExpandoClone(ExpandoObject original)
        {
            var expandoObjectConverter = new ExpandoObjectConverter();
            var originalDoc = JsonConvert.SerializeObject(original, expandoObjectConverter);

            dynamic clone = JsonConvert.DeserializeObject<ExpandoObject>(originalDoc, expandoObjectConverter);

            return clone;
        }
    }
}