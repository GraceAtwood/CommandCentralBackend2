using CommandCentral.Enums;
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.CLI
{
    public class BuildOptions
    {
        /// <summary>
        /// The last state of the parser prior to an error occurring.
        /// </summary>
        [ParserState]
        public IParserState LastParserState { get; set; }

        /// <summary>
        /// Returns the usage information for the launch parameters.
        /// </summary>
        /// <param name="verb"></param>
        /// <returns></returns>
        [HelpOption]
        public string GetUsage()
        {
            var help = HelpText.AutoBuild(this);
            help.Heading = new HeadingInfo("Command Central Service CLI", "1.0.0");
            help.Copyright = new CopyrightInfo(true, "U.S. Navy", 2017);
            help.AdditionalNewLineAfterOption = true;
            help.AddDashesToOption = true;

            help.AddPreOptionsLine("License: IDK.");

            return help;
        }
    }
}
