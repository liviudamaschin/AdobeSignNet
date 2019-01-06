using System.Collections.Generic;

namespace AdobeSignRESTClient.Models
{
    public class FormFieldComplete
    {
        public List<FormLocation> locations { get; set; }
        public string name { get; set; }
        public string alignment { get; set; }
        public string assignee { get; set; }
        public string backgroundColor { get; set; }
        public string borderColor { get; set; }
        public string borderStyle { get; set; }
        public int borderWidth { get; set; }
        public bool calculated { get; set; }
        public FormConditionalAction conditionalAction { get; set; }
        public string contentType { get; set; }
        public string defaultValue { get; set; }
        public string displayFormat { get; set; }
        public string displayFormatType { get; set; }
        public string displayLabel { get; set; }
        public string fontColor { get; set; }
        public string fontName { get; set; }
        public int fontSize { get; set; }
        public List<string> hiddenOptions { get; set; }
        public FormHyperlink hyperlink { get; set; }
        public string inputType { get; set; }
        public bool masked { get; set; }
        public string maskingText { get; set; }
        public string maxLength { get; set; }
        public int maxValue { get; set; }
        public string minLength { get; set; }
        public int minValue { get; set; }
        public string origin { get; set; }
        public string radioCheckType { get; set; }
        public bool readOnly { get; set; }
        public bool required { get; set; }
        public string tooltip { get; set; }
        public bool urlOverridable { get; set; }
        public string validation { get; set; }
        public string validationData { get; set; }
        public string validationErrMsg { get; set; }
        public string valueExpression { get; set; }
        public bool visible { get; set; }
        public List<string> visibleOptions { get; set; }
    }
}
