namespace DOMMultipleValuesExtractor_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.Modules;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.ManagerStore;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;

	[GQIMetaData(Name = "DOM Multiple Values Extractor")]
	public class DOMMultipleValuesExtractor : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		private const string InputName = "Module - Section - Field";
		private const string InputGuid = "Guid";
		private readonly List<Options> _options = new List<Options>();

		private GQIStringDropdownArgument _inputName;
		private GQIStringArgument _inputGuid;

		private Func<DMSMessage[], DMSMessage[]> _dmsMessages;
		private ModuleSettingsHelper _moduleSettingsHelper;

		private string _result = string.Empty;

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			_dmsMessages = args.DMS.SendMessages;
			_moduleSettingsHelper = new ModuleSettingsHelper(args.DMS.SendMessages);

			return default;
		}

		public GQIArgument[] GetInputArguments()
		{
			CreateOptions();

			_inputName = new GQIStringDropdownArgument(InputName, _options.ToSelection()) { IsRequired = true };
			_inputGuid = new GQIStringArgument(InputGuid) { IsRequired = false };

			return new GQIArgument[2]
			{
				_inputName,
				_inputGuid,
			};
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			var inputName = args.GetArgumentValue(_inputName);
			if (!args.TryGetArgumentValue(_inputGuid, out string inputGuid))
				return default;

			Options selectedOption = _options.FirstOrDefault(option => option.ToString() == inputName);
			if (selectedOption == default)
				return default;

			var domHelper = new DomHelper(_dmsMessages, selectedOption.Module);

			var fieldFilter = DomInstanceExposers.FieldValues.DomInstanceField(selectedOption.FieldDescriptorId).NotEqual(string.Empty);
			var guidFilter = DomInstanceExposers.Name.Equal(inputGuid);
			var instance = domHelper.DomInstances.Read(fieldFilter.AND(guidFilter)).FirstOrDefault();
			if (instance == null)
				return default;

			_result = instance.Sections.FirstOrDefault(section => section.SectionDefinitionID.Id == selectedOption.SectionDefinitionId.Id)?.GetFieldValueById(selectedOption.FieldDescriptorId)?.Value.ToString();

			return default;
		}

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[1]
			{
				new GQIStringColumn("GUID"),
			};
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			var rows = new List<GQIRow>();

			if (_result.IsNullOrEmpty())
				return new GQIPage(rows.ToArray());

			foreach (var result in _result.Split(';'))
			{
				rows.Add(new GQIRow(
					new GQICell[]
					{
						new GQICell()
						{
							Value = result,
						},
					}));
			}

			return new GQIPage(rows.ToArray());
		}

		private void CreateOptions()
		{
			List<ModuleSettings> modulesSettings = _moduleSettingsHelper.ModuleSettings.ReadAll();

			foreach (var moduleSettings in modulesSettings)
			{
				var domHelper = new DomHelper(_dmsMessages, moduleSettings.ModuleId);

				var sectionDefinitions = domHelper.SectionDefinitions.ReadAll();

				foreach (var sectionDefinition in sectionDefinitions)
				{
					foreach (var fieldDescriptor in sectionDefinition.GetAllFieldDescriptors())
					{
						if (fieldDescriptor.FieldType == typeof(List<Guid>))
						{
							_options.Add(new Options(moduleSettings.ModuleId, sectionDefinition.GetName(), fieldDescriptor.Name, sectionDefinition.GetID(), fieldDescriptor.ID));
						}
					}
				}
			}
		}
	}
}