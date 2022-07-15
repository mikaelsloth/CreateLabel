namespace CreateLabel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal abstract class UiController
    {
        protected string ApiName = "-";

        public virtual InputModeEnum InputMode { get; } = InputModeEnum.Request;

        public virtual string LabelForRequestSchema => $"Validate {ApiName} Request Schema";

        public virtual string LabelForInput => $"Paste your {InputMode} below";

        public virtual string RequestSchema { get; set; } = "";

        public virtual string RequestSchemaPath { get; set; } = "";

        public virtual bool ValidateRequestSchema { get; set; }

        public virtual string SaveOutputPath { get; set; } = "";

        public virtual bool ShowReponse { get; set; }

        public abstract void InitializeFormValues();

        public abstract void RunCommand();

        public abstract void DisplayResponse();
    }
}
