namespace DOMMultipleValuesExtractor_1
{
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Sections;

	public struct Options
	{
		public Options(string module, string section, string field, SectionDefinitionID sectionDefinitionId, FieldDescriptorID fieldDescriptorId)
		{
			Module = module;
			Section = section;
			Field = field;
			SectionDefinitionId = sectionDefinitionId;
			FieldDescriptorId = fieldDescriptorId;
		}

		public string Module { get; set; }

		public string Section { get; set; }

		public string Field { get; set; }

		public SectionDefinitionID SectionDefinitionId { get; set; }

		public FieldDescriptorID FieldDescriptorId { get; set; }

		public static bool operator ==(Options o1, Options o2)
		{
			return o1.Equals(o2);
		}

		public static bool operator !=(Options o1, Options o2)
		{
			return !(o1 == o2);
		}

		public bool Equals(Options o)
		{
			return
				Module == o.Module
				&& Section == o.Section
				&& Field == o.Field
				&& SectionDefinitionId == o.SectionDefinitionId
				&& FieldDescriptorId == o.FieldDescriptorId;
		}

		public override bool Equals(object obj) => obj is Options other && Equals(other);

		public override int GetHashCode() => (Module, Section, Field, SectionDefinitionId, FieldDescriptorId).GetHashCode();

		public override string ToString()
		{
			return $"{Module} - {Section} - {Field}";
		}
	}

	public static class Extensions
	{
		public static string[] ToSelection(this List<Options> options)
		{
			return options.Select(option => option.ToString()).ToArray();
		}
	}
}