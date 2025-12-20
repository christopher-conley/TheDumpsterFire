using System;
using System.Collections.Generic;
using System.Text;

namespace PSPhlebotomist.Core
{
    /// <summary>
    /// Represents the results and details of an injection attempt on a process, including success status, counts,
    /// and pluralized labels.
    /// </summary>
    /// <remarks>Use this class to track the outcome of multiple injection attempts in a single run, including the number of
    /// successes and failures, and to access details for each individual attempt. The plural form properties assist in
    /// generating grammatically correct output for reporting purposes.</remarks>
    public class PatientDiagnosis
    {
        private string _payloadPlural = string.Empty;
        private string _failurePlural = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the injection completed successfully.
        /// </summary>
        public bool InoculationSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the total number of DLLs/PE images that were attempted to be injected into the target process.
        /// </summary>
        public int InoculumsAttempted { get; set; }

        /// <summary>
        /// Gets or sets the number of injections that completed successfully.
        /// </summary>
        public int InoculumsSucceeded { get; set; }

        /// <summary>
        /// Gets or sets the number of injection attempts that failed.
        /// </summary>
        public int InoculumsFailed { get; set; }

        /// <summary>
        /// Gets or sets the collection of DLL/PE images (path), along with their injection success/fail status.
        /// </summary>
        /// <remarks>Each key in the dictionary represents the path to a DLL/PE image, and the
        /// corresponding value indicates whether the injecion attempt for that image was successful or not.</remarks>
        public Dictionary<string, bool> InoculumsDetail { get; set; } = new();

        /// <summary>
        /// Gets grammatically correct plural form for payloads.
        /// </summary>
        public string PayloadPlural
        {
            get => _payloadPlural;
            protected internal set => _payloadPlural = value;
        }

        /// <summary>
        /// Gets grammatically correct plural form for failures.
        /// </summary>
        public string FailurePlural
        {
            get => _failurePlural;
            protected internal set => _failurePlural = value;
        }


        /// <summary>
        /// Gets exit code based on failures (0 = all succeeded).
        /// </summary>
        public int ExitCode => InoculumsFailed;
    }
}
