//  Authors:    Robert M. Scheller, James B. Domingo

using Landis.Utilities;
using Landis.Core;
using System.Collections.Generic;
using System.Text;
using System;

namespace Landis.Extension.BaseHurricane
{
    /// <summary>
    /// A parser that reads the plug-in's parameters from text input.
    /// </summary>
    public class InputParameterParser
        : TextParser<IInputParameters>
    {
        public override string LandisDataValue
        {
            get
            {
                return PlugIn.ExtensionName;
            }
        }

        //---------------------------------------------------------------------

        public InputParameterParser()
        {
            // FIXME: Hack to ensure that Percentage is registered with InputValues
            Landis.Utilities.Percentage p = new Landis.Utilities.Percentage();
        }

        //---------------------------------------------------------------------

        protected override IInputParameters Parse()
        {
            string[] parseLine(string line)
            {
                return line.Replace("\t", " ")
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            var stormOccurProb = "StormOccurrenceProbabilities";
            var inputUnitsEnglish = "InputUnitsEnglish";
            var lowBoundLandfallWindSpeed = "LowBoundLandfallWindSpeed";
            var modeLandfallWindSpeed = "ModeLandfallWindSpeed";
            var highBoundLandfallWindSpeed = "HighBoundLandfallWindSpeed";
            var centerPointLatitude = "CenterPointLatitude";
            var centerPointDistanceInland = "CenterPointDistanceInland";
            var windSeverities = "WindSeverities";
            var windSpeedVuln = "WindSpeedVulnerabilities";
            var fortBragg = "FortBragg";



            var sectionNames = new HashSet<System.String> {stormOccurProb, 
                windSeverities, windSpeedVuln, fortBragg };

            var singleLineNames = new HashSet<System.String> {lowBoundLandfallWindSpeed,
                modeLandfallWindSpeed, highBoundLandfallWindSpeed,
                centerPointLatitude, centerPointDistanceInland, inputUnitsEnglish,};
            var inputUnitsAreEnglish = false;

            ReadLandisDataVar();

            InputParameters parameters = new InputParameters(PlugIn.ModelCore.Ecoregions.Count);

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            // Read the Storm Occurrence Probabilities table
            // The call to ReadVar advanced the cursor, so it contains the next line.
            string lastOperation = this.CurrentName;
            while(sectionNames.Contains(lastOperation) || singleLineNames.Contains(lastOperation))
            {
                string[] row;
                if(sectionNames.Contains(lastOperation))
                {
                    sectionNames.Remove(lastOperation);
                    GetNextLine();
                    if(lastOperation == stormOccurProb)
                    {
                        populateStormOccurenceProbabilities(
                            parameters, sectionNames, singleLineNames, parseLine);
                    }
                    else if (lastOperation == windSpeedVuln)
                    {
                        populateWindSpeedVulnverabilities(
                            parameters, sectionNames, singleLineNames, parseLine);
                    }
                    lastOperation = this.CurrentName;
                }

                if(singleLineNames.Contains(lastOperation))
                {
                    string value = "";
                    row = parseLine(this.CurrentLine);
                    if(row.Length >= 2)
                        value = row[1];
                    if(lastOperation == lowBoundLandfallWindSpeed)
                        parameters.LowBoundLandfallWindSpeed = Convert.ToDouble(value);
                    else if(lastOperation == modeLandfallWindSpeed)
                        parameters.ModeLandfallWindSpeed = Convert.ToDouble(value);
                    else if(lastOperation == highBoundLandfallWindSpeed)
                        parameters.HighBoundLandfallWindspeed = Convert.ToDouble(value);
                    else if(lastOperation == centerPointLatitude)
                        parameters.CenterPointLatitude = Convert.ToDouble(value);
                    else if(lastOperation == centerPointDistanceInland)
                        parameters.CenterPointDistanceInland = Convert.ToDouble(value);
                    else if(lastOperation == inputUnitsEnglish)
                        inputUnitsAreEnglish = true;
                    singleLineNames.Remove(lastOperation);
                    GetNextLine();
                    lastOperation = this.CurrentName;
                }
                if(lastOperation == windSeverities) break;
                if(lastOperation == fortBragg) break;
            }
            if(inputUnitsAreEnglish)
                parameters.AdjustValuesFromEnglishToMetric();


            //  Read table of wind event parameters for ecoregions
            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion");
            InputVar<double> maxSize = new InputVar<double>("Max Size");
            InputVar<double> meanSize = new InputVar<double>("Mean Size");
            InputVar<double> minSize = new InputVar<double>("Min Size");
            InputVar<int> rotationPeriod = new InputVar<int>("Rotation Period");

            Dictionary <string, int> lineNumbers = new Dictionary<string, int>();
            const string WindSeverities = "WindSeverities";

            while (! AtEndOfInput && CurrentName != WindSeverities) {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(ecoregionName, currentLine);
                IEcoregion ecoregion = PlugIn.ModelCore.Ecoregions[ecoregionName.Value.Actual];
                if (ecoregion == null)
                    throw new InputValueException(ecoregionName.Value.String,
                                                  "{0} is not an ecoregion name.",
                                                  ecoregionName.Value.String);
                int lineNumber;
                if (lineNumbers.TryGetValue(ecoregion.Name, out lineNumber))
                    throw new InputValueException(ecoregionName.Value.String,
                                                  "The ecoregion {0} was previously used on line {1}",
                                                  ecoregionName.Value.String, lineNumber);
                else
                    lineNumbers[ecoregion.Name] = LineNumber;

                IEventParameters eventParms = new EventParameters();
                parameters.EventParameters[ecoregion.Index] = eventParms;

                ReadValue(maxSize, currentLine);
                eventParms.MaxSize = maxSize.Value;

                ReadValue(meanSize, currentLine);
                eventParms.MeanSize = meanSize.Value;

                ReadValue(minSize, currentLine);
                eventParms.MinSize = minSize.Value;

                ReadValue(rotationPeriod, currentLine);
                eventParms.RotationPeriod = rotationPeriod.Value;

                CheckNoDataAfter("the " + rotationPeriod.Name + " column",
                                 currentLine);
                GetNextLine();
            }

            //  Read table of wind severities.
            //  Severities are in decreasing order.
            ReadName(WindSeverities);

            InputVar<byte> number = new InputVar<byte>("Severity Number");
            InputVar<Percentage> minAge = new InputVar<Percentage>("Min Age");
            InputVar<Percentage> maxAge = new InputVar<Percentage>("Max Age");
            InputVar<float> mortProbability = new InputVar<float>("Mortality Probability");

            const string MapNames = "MapNames";
            byte previousNumber = 6;
            Percentage previousMaxAge = null;

            while (! AtEndOfInput && CurrentName != MapNames
                                  && previousNumber != 1) {
                StringReader currentLine = new StringReader(CurrentLine);

                ISeverity severity = new Severity();
                parameters.WindSeverities.Add(severity);

                ReadValue(number, currentLine);
                severity.Number = number.Value;

                //  Check that the current severity's number is 1 less than
                //  the previous number (numbers are must be in decreasing
                //  order).
                if (number.Value.Actual != previousNumber - 1)
                    throw new InputValueException(number.Value.String,
                                                  "Expected the severity number {0}",
                                                  previousNumber - 1);
                previousNumber = number.Value.Actual;

                ReadValue(minAge, currentLine);

                severity.MinAge = (double) minAge.Value.Actual;

                if (parameters.WindSeverities.Count == 1) {
                    //  Minimum age for this severity must be equal to 0%
                    if (minAge.Value.Actual != 0)
                        throw new InputValueException(minAge.Value.String,
                                                      "It must be 0% for the first severity");
                }
                else {
                    //  Minimum age for this severity must be equal to the
                    //  maximum age of previous severity.
                    if (minAge.Value.Actual != (double) previousMaxAge)
                        throw new InputValueException(minAge.Value.String,
                                                      "It must equal the maximum age ({0}) of the preceeding severity",
                                                      previousMaxAge);
                }

                TextReader.SkipWhitespace(currentLine);
                string word = TextReader.ReadWord(currentLine);
                if (word != "to") {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Expected \"to\" after the minimum age ({0})",
                                         minAge.Value.String);
                    if (word.Length > 0)
                        message.AppendFormat(", but found \"{0}\" instead", word);
                    throw NewParseException(message.ToString());
                }

                ReadValue(maxAge, currentLine);
                severity.MaxAge = (double) maxAge.Value.Actual;
                if (number.Value.Actual == 1) {
                    //  Maximum age for the last severity must be 100%
                    if (maxAge.Value.Actual != 1)
                        throw new InputValueException(minAge.Value.String,
                                                      "It must be 100% for the last severity");
                }
                previousMaxAge = maxAge.Value.Actual;

                ReadValue(mortProbability, currentLine);
                severity.MortalityProbability = mortProbability.Value;

                CheckNoDataAfter("the " + mortProbability.Name + " column",
                                 currentLine);
                GetNextLine();
            }
            if (parameters.WindSeverities.Count == 0)
                throw NewParseException("No severities defined.");
            if (previousNumber != 1)
                throw NewParseException("Expected hurricane wind severity {0}", previousNumber - 1);

            InputVar<string> mapNames = new InputVar<string>(MapNames);
            ReadVar(mapNames);
            parameters.MapNamesTemplate = mapNames.Value;

            InputVar<string> logFile = new InputVar<string>("LogFile");
            ReadVar(logFile);
            parameters.LogFileName = logFile.Value;

            CheckNoDataAfter(string.Format("the {0} parameter", logFile.Name));

            return parameters; //.GetComplete();
        }

        private void populateWindSpeedVulnverabilities(
            InputParameters parameters, 
            HashSet<string> sectionNames, HashSet<string> singleLineNames, 
            Func<string, string[]> parseLine)
        {
            string[] aRow;
            parameters.windSpeedVulnverabilities =
                new Dictionary<string, Dictionary<double, Dictionary<double, double>>>();

            while(!(sectionNames.Contains(this.CurrentName) ||
                singleLineNames.Contains(this.CurrentName)))
            {
                aRow = parseLine(this.CurrentLine);
                string speciesName = aRow[0];
                if(!parameters.windSpeedVulnverabilities.ContainsKey(speciesName))
                    parameters.windSpeedVulnverabilities[speciesName] =
                        new Dictionary<double, Dictionary<double, double>>();

                double age = Convert.ToDouble(aRow[1]);
                var dataVals = aRow.SliceToEnd(2);
                Dictionary<double, double> probabilities = new Dictionary<double, double>();
                foreach(var entry in dataVals)
                {
                    var split = entry.Split(':');
                    var speed = Convert.ToDouble(split[0]);
                    var probability = Convert.ToDouble(split[1]);
                    probabilities[speed] = probability;
                }
                var cohortAges = new Dictionary<double, Dictionary<double, double>>();
                cohortAges[age] = probabilities;

                parameters.windSpeedVulnverabilities[speciesName][age] = probabilities;

                //parameters.windSpeedVulnverabilities;
                GetNextLine();
            }
        }

        private void populateStormOccurenceProbabilities(
            InputParameters parameters, HashSet<string> sectionNames, 
            HashSet<string> singleLineNames, Func<string, string[]> parseLine)
        {
            string[] aRow;
            parameters.StormOccurenceProbabilities = new List<double>();
            while(!(sectionNames.Contains(this.CurrentName) ||
                singleLineNames.Contains(this.CurrentName)))
            {
                aRow = parseLine(this.CurrentLine);
                parameters.StormOccurenceProbabilities.Add(Convert.ToDouble(aRow[1]));
                GetNextLine();
            }
        }
    }

    public static class ExtensionMethods
    {
        public static List<string> SliceToEnd(this string[] strArray, int startIdx)
        {
            var len = strArray.Length;
            var retList = new List<string>(strArray);
            retList = retList.GetRange(startIdx, len - startIdx);
            return retList;

        }
    }
}
