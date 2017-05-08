using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nwmon
{
    class Options
    {
        [Option('d', "download", DefaultValue = 3, HelpText = "Download speed limit monitor, Measure in Megabit per second (Mbps)")]
        public double Download { get; set; }
        [Option('u', "upload", DefaultValue = 3, HelpText = "Upload speed limit monitor, Measure in Megabit per second (Mbps)")]
        public double Upload { get; set; }
        [Option('f', "force", DefaultValue = false, HelpText = "Force bypass sound notification when download/upload is exceed limit")]
        public bool Force { get; set; }
        [Option('i', "interval", DefaultValue = 1000, HelpText = "Refresh rate interval less value is more likely to be accurate at usage per second measurement but may affect total usage measurement and system performance (IF YOU DON'T KNOW WHAT IS THIS ABOUT, DON'T MESS WITH IT)")]
        public int Interval { get; set; }
        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
