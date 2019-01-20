// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.BotBuilderSamples
{

    /// <summary>
    /// For each interaction from the user, an instance of this class is created and
    /// the OnTurnAsync method is called.
    /// This is a transient lifetime service. Transient lifetime services are created
    /// each time they're requested. For each <see cref="Activity"/> received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    /// <seealso cref="https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.ibot?view=botbuilder-dotnet-preview"/>
    public class LuisBot : IBot
    {
        /// <summary>
        /// Key in the bot config (.bot file) for the LUIS instance.
        /// In the .bot file, multiple instances of LUIS can be configured.
        /// </summary>
        public static readonly string LuisKey = "nullteam";

        private const string WelcomeText = "This bot will introduce you to natural language processing with LUIS. Type an utterance to get started";

        /// <summary>
        /// Services configured from the ".bot" file.
        /// </summary>
        ///         private readonly LuisBotAccessors _accessors;
        private readonly LuisBotAccessors _accessors;

        private readonly BotServices _services;
        private DialogSet _dialogs;



        /// <summary>
        /// Initializes a new instance of the <see cref="LuisBot"/> class.
        /// </summary>
        /// <param name="services">Services configured from the ".bot" file.</param>
        public LuisBot(LuisBotAccessors accessors, BotServices services)
        {
            _services = services ?? throw new System.ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            if (!_services.LuisServices.ContainsKey(LuisKey))
            {
                throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a LUIS service named '{LuisKey}'.");
            }

            _dialogs = new DialogSet(accessors.ConversationDialogState);

            var waterfallSteps = new WaterfallStep[]
            {
                LocationStepAsync,
                SavedClinicStepAsync,
                DisplayClinicStepAsync,
            };

            var setClinicSteps = new WaterfallDialog[] {
                 
            };

            _dialogs.Add(new WaterfallDialog("details", waterfallSteps));
            _dialogs.Add(new TextPrompt("location"));
            _dialogs.Add(new NumberPrompt<int>("selected"));

            _dialogs.Add(new WaterfallDialog("setclinic", waterfallSteps));


        }

        private static async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            return await stepContext.PromptAsync("location", new PromptOptions { Prompt = MessageFactory.Text("You do not have a pre-set clinic, we will suggest clinics near you based on your GPS location.") }, cancellationToken);

        }

        private async Task<DialogTurnResult> SavedClinicStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            userProfile.Location = (string)stepContext.Result;
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text(userProfile.Location), cancellationToken);
            //List<List<string>> lll = GetClinics();
            string clinics = @"Southern Calif Medical Group|BCC|FRONTLINE Provider/Occupational Industrial Clinic, Multi-Specialty|3320 S Hill St|Los Angeles|Los Angeles|CA|90007|(213) 749-5386|-118.2744516|34.0197291
Stacy Medical Center|BCC|FRONTLINE Provider/Occupational Industrial Clinic, Multi-Specialty|4580 Pacific Blvd|Vernon|Los Angeles|CA|90058|(323) 584-0779|-118.2248393|34.0019771
Concentra Medical Center|BCC|FRONTLINE Provider/Occupational Industrial Clinic, Multi-Specialty|1101 S Milliken Ave Ste C|Ontario|San Bernardino|CA|91761|(909) 390-2799|-117.557599|34.0531719
Baumgartner, Richard|BCC|Family Practice|42002 Fox Farm Rd Ste 200|Big Bear Lake|San Bernardino|CA|92315|(909) 866-5808|-116.886021|34.248886
Parsa, A|BCC|Family Practice, General Practice|2864 E Imperial Hwy|Brea|Orange|CA|92821|(714) 996-9708|-117.8588218|33.9105575
Concentra Medical Center|BCC|FRONTLINE Provider/Occupational Industrial Clinic, Multi-Specialty|640 S Placentia Ave|Placentia|Orange|CA|92870|(714) 579-7772|-117.881089|33.863446
Ventura Urgent Care Center Inc|BCC|FRONTLINE Provider/Occupational Industrial Clinic, Emergency Medicine|5725 Ralston St Ste 101|Ventura|Ventura|CA|93003|(805) 658-2273|-119.2136281|34.2610761
Acevedo, Alberto|BCC|General Practice|1205 Garces Hwy Ste 102|Delano|Kern|CA|93215|(661) 721-3530|-119.240218|35.7620041
Central Valley Occupational Medical Group|BCC|FRONTLINE Provider/Occupational Industrial Clinic, Multi-Specialty|4100 Truxtun Ave Ste 200|Bakersfield|Kern|CA|93309|(661) 632-1540|-119.0459538|35.3735079
Concentra Medical Center|BCC|FRONTLINE Provider/Occupational Industrial Clinic, Multi-Specialty|509 South I St Ste A|Madera|Madera|CA|93637|(559) 673-9020|-120.0587149|36.9547688
Sekhon, Shobha|BCC|Family Practice|820 E Almond Ave|Madera|Madera|CA|93637|(559) 674-8787|-120.0484229|36.9452381
Concentra Medical Center|BCC|FRONTLINE Provider/Occupational Industrial Clinic, Multi-Specialty|2555 S East Ave|Fresno|Fresno|CA|93706|(559) 499-2400|-119.7722122|36.70912670000001
Concentra Medical Center|BCC|FRONTLINE Provider/Occupational Industrial Clinic, Multi-Specialty|7265 N 1st St Ste 105|Fresno|Fresno|CA|93720|(559) 431-8181|-119.7767128|36.8424281
Salinas Urgent Care|BCC|FRONTLINE Provider/Occupational Industrial Clinic, Multi-Specialty, Urgent Care Clinic|558 Abbott St Ste A|Salinas|Monterey|CA|93901|(831) 755-7880|-121.6426036|36.6624374
Steimnitz, Jules|BCC|Physical Medicine & Rehabilitation|1580 Valencia St Ste 210|San Francisco|San Francisco|CA|94110|(415) 641-8631|-122.4205566|37.7467497
Concentra Medical Center|BCC|FRONTLINE Provider/Occupational Industrial Clinic, Multi-Specialty|2587 Merced St|San Leandro|Alameda|CA|94577|(510) 351-3553|-122.1685347|37.7041932
Concentra Medical Center|BCC|FRONTLINE Provider/Occupational Industrial Clinic, Multi-Specialty|384 Embarcadero W|Oakland|Alameda|CA|94607|(510) 465-9565|-122.2751806|37.7949969
Concentra Medical Center|BCC|FRONTLINE Provider/Occupational Industrial Clinic, Multi-Specialty|2970 Hilltop Mall Rd Ste 203|Richmond|Contra Costa|CA|94806|(510) 222-8000|-122.3292634|37.977616
Anderson Valley Health Center|BCC|Multi-Specialty|13500 Airport Rd|Boonville|Mendocino|CA|95415|(707) 895-3477|-123.3792526|39.0132151
Andolsen, Richard|BCC|Family Practice|465 March Ave Ste A|Healdsburg|Sonoma|CA|95448|(707) 433-3369|-122.8639947|38.6262256
Levin, Daniel|BCC|Oral & Maxillo-Facial Surgery|7891 Talbert Ave Ste 101|Huntington Beach|Orange|CA|92648|(714) 842-2521|-117.9904177|33.7015718
Bhatt, Jatin|BCC|Family Practice, Internal Medicine|3628 E Imperial Hwy Ste 303|Lynwood|Los Angeles|CA|90262|(310) 900-2790|-118.2042088|33.9304634
Koudsi, Nabil|BCC|Surgery, General Vascular Surgery|811 E 11th St Ste 207|Upland|San Bernardino|CA|91786|(909) 981-3411|-117.639854|34.1037418
Cotten, Paul|BCC|Anesthesiology|2185 W Citracado Pkwy|Escondido|San Diego|CA|92025|(442) 281-1000, (858) 673-6100|-117.1218794|33.1218035
Flint, Michael|BCC|Chiropractor|3587 Meade Ave|San Diego|San Diego|CA|92116|(619) 283-5963|-117.1159936|32.7569819
Joshi, Chandrashekhar|BCC|Internal Medicine|1350 E Los Angeles Ave|Simi Valley|Ventura|CA|93065|(805) 522-3782|-118.7729442|34.2714291
Behl, Ashok|BCC|Cardiovascular Disease|567 W Putnam Ave Ste 1|Porterville|Tulare|CA|93257|(559) 781-0386|-119.0296869|36.0693754
Hernandez, Louis|BCC|Family Practice, General Practice|405 Riverside Dr|Madera|Madera|CA|93638|(559) 661-0247|-120.0628901|36.968807
Klein, Steven|BCC|Podiatry|139 Arch St Ste 1|Redwood City|San Mateo|CA|94062|(650) 366-3668|-122.2377141|37.4877572
Anderson, Christian|BCC|Radiology|1000 Trancas St|Napa|Napa|CA|94558|(707) 252-4411|-122.2971094|38.324482
Chen, Howard|BCC|Ophthalmology|1663 Dominican Way Ste 110-A|Santa Cruz|Santa Cruz|CA|95065|(650) 257-3861|-121.9808093|36.9907056
Dessouki, Amr|BCC|Ophthalmology|1663 Dominican Way Ste 110-A|Santa Cruz|Santa Cruz|CA|95065|(831) 476-5888|-121.9808093|36.9907056
Eureka Physical Therapy|BCC|Physical Therapy|2306 Dean St|Eureka|Humboldt|CA|95501|(707) 443-8354|-124.1418314|40.7866862
Gramm, Gary|BCC|Family Practice|6135 King Rd Ste A|Loomis|Placer|CA|95650|(916) 652-0427|-121.1891583|38.825341
Banker, Dennis|BCC|Chiropractor|9230 Bruceville Rd Ste 2|Elk Grove|Sacramento|CA|95758|(916) 683-7000|-121.418698|38.4224112
Heyerman, William|BCC|Orthopedic Surgery|2160 Court St Ste B|Redding|Shasta|CA|96001|(530) 244-2663|-122.3934927|40.5768925
Larchmont Physical Therapy|BCC|Physical Therapy|321 N Larchmont Blvd Ste 825|Los Angeles|Los Angeles|CA|90004|(323) 464-4458|-118.3240088|34.07686899999999
Fitzgerald, Theresa|BCC|Chiropractor|2904 Rowena Ave|Los Angeles|Los Angeles|CA|90039|(323) 660-2370|-118.2682622|34.1081662
Clark, Stevan|BCC|Colon & Rectal Surgery, General Practice, Surgery|10220 S Western Ave|Los Angeles|Los Angeles|CA|90047|(323) 757-1744|-118.3086373|33.9433455
Gelman, Ilya|BCC|Internal Medicine, Nephrology|6333 Wilshire Blvd Ste 200|Los Angeles|Los Angeles|CA|90048|(323) 653-2504|-118.3663618|34.0638923
Bailey, Joselyn|BCC|Internal Medicine|4305 Torrance Blvd Ste 506|Torrance|Los Angeles|CA|90503|(310) 542-7341|-118.3593143|33.8385365
Melikian, Robert|BCC|Internal Medicine|3801 Katella Ave Ste 321|Los Alamitos|Orange|CA|90720|(562) 594-8149|-118.0658561|33.80340109999999
Abrams, Robert|BCC|Podiatry|24355 Lyons Ave Ste 105|Santa Clarita|Los Angeles|CA|91321|(661) 253-3668|-118.560601|34.3790362
Kantor, Richard|BCC|Chiropractor|27600 Bouquet Canyon Rd Ste 106|Santa Clarita|Los Angeles|CA|91350|(661) 296-2131|-118.5119375|34.4422881
Chanes, Luis|BCC|Ophthalmology|2621 S Bristol St Ste 205|Santa Ana|Orange|CA|92704|(714) 557-5777|-117.8847571|33.7119163
Linnemann, Gary|BCC|Family Practice, Occupational Medicine|1534 E Warner Ave Ste A|Santa Ana|Orange|CA|92705|(714) 557-5599|-117.8480392|33.7154042
Devereaux, Robert|BCC|Family Practice|11100 Warner Ave Ste 100|Fountain Valley|Orange|CA|92708|(714) 957-9389|-117.9356096|33.7156998
Li, Jackie|BCC|Software Engineer|340 S Brody Rd|East Lansing|Ingham County|MI|48825|(517)-402-5441|-84.4946926|42.7303183
Kye, Kevin|BCC|Software Engineer|591 N Shaw Ln|East Lansing|Ingham County|MI|48825|(616) 308 6951|-84.47529109999999|42.7267794";
            double userlon = Convert.ToDouble(userProfile.Location.Split("|")[0]);
            double userlat = Convert.ToDouble(userProfile.Location.Split("|")[1]);
            List<List<string>> all_clinic = new List<List<string>>();
            List<string> line = new List<string>(clinics.Split("\r\n"));
            foreach (string i in line)
            {
                List<string> nn = new List<string>(i.Split("|"));
                all_clinic.Add(nn);
            }

            for (var i = 0; i < all_clinic.Count; i++) {
                var clinic = all_clinic[i];
                double lon = Convert.ToDouble(clinic[9]);
                double lat = Convert.ToDouble(clinic[10]);
                double dist = DistanceTo(lat, lon, userlat, userlon);
                all_clinic[i].Add(dist.ToString());
                Console.WriteLine(dist.ToString());
            }

            Console.WriteLine(all_clinic[0][0]);
            all_clinic.Sort((x, y) => Convert.ToInt32((Convert.ToDouble(x[11]) - Convert.ToDouble(y[11])) * 1000));

            string reply = string.Empty;

            for (var i = 0; i < 3; i++) {
                var item = all_clinic[i];
                for (var x = 0; x < item.Count; x++) { 
                    reply += item[x];
                    if (x != item.Count - 1)
                    {
                        reply += "|";
                    }
                }
                reply += "\n";
            }

            return await stepContext.PromptAsync("selected", new PromptOptions { Prompt = MessageFactory.Text(reply) }, cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayClinicStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            userProfile.Option = (int)stepContext.Result;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Here is the information about selected clinic."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Every conversation turn for our LUIS Bot will call this method.
        /// There are no dialogs used, the sample only uses "single turn" processing,
        /// meaning a single request and response, with no stateful conversation.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Run the DialogSet - let the framework identify the current state of the dialog from
                // the dialog stack and figure out what (if any) is the active dialog.
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                // If the DialogTurnStatus is Empty we should start a new dialog.
                if (results.Status == DialogTurnStatus.Empty)
                {

                    var recognizerResult = await _services.LuisServices[LuisKey].RecognizeAsync(turnContext, cancellationToken);
                    var topIntent = recognizerResult?.GetTopScoringIntent();
                    if (topIntent != null && topIntent.HasValue && topIntent.Value.intent != "None") {
                        if(turnContext.Activity.Text == "welcome")
                        {
                            await turnContext.SendActivityAsync("Welcome to Team Null's clinic finder bot, you can try to ask me \"Find me a clinic\" or \"Set my clinic\".");

                        }
                        else if (topIntent.Value.score < 0.5)
                        {
                            await turnContext.SendActivityAsync("I do not understand your question, you can try to ask \"Find me a clinic\".");
                        }
                        else {
                            switch (topIntent.Value.intent)
                            {
                                case "clinic_find":
                                    await dialogContext.BeginDialogAsync("details", null, cancellationToken);
                                    break;
                                case "clinic_set":
                                    await turnContext.SendActivityAsync("setclinic");
                                    break;
                                default:
                                    await turnContext.SendActivityAsync("I do not understand your question, you can try to ask \"Find me a clinic\".");
                                    break;
                            }
                        }
                    }

                }
            }

            // Processes ConversationUpdate Activities to welcome the user.
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                //if (turnContext.Activity.MembersAdded != null)
                //{
                //    // Iterate over all new members added to the conversation
                //    foreach (var member in turnContext.Activity.MembersAdded)
                //    {
                //        // Greet anyone that was not the target (recipient) of this message
                //        // the 'bot' is the recipient for events from the channel,
                //        // turnContext.Activity.MembersAdded == turnContext.Activity.Recipient.Id indicates the
                //        // bot was added to the conversation.
                //        if (member.Id != turnContext.Activity.Recipient.Id)
                //        {
                //            await turnContext.SendActivityAsync("Welcome to Team Null's MPN assistant, please enter/say your question.");
                //        }
                //    }
                //}
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected", cancellationToken: cancellationToken);
            }

            // Save the dialog state into the conversation state.
            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

            // Save the user profile updates into the user state.
            await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// On a conversation update activity sent to the bot, the bot will
        /// send a message to the any new user(s) that were added.
        /// </summary>
        /// <param name="turnContext">Provides the <see cref="ITurnContext"/> for the turn of the bot.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>>A <see cref="Task"/> representing the operation result of the Turn operation.</returns>
        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    //await turnContext.SendActivityAsync(
                    //   $"Welcome to LuisBot {member.Name}. {WelcomeText}",
                    //  cancellationToken: cancellationToken);
                    await turnContext.SendActivityAsync("Welcome to Team Null's MPN assistant, please enter/say your question.");
                }
            }
        }
        public static double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'M')
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (unit)
            {
                case 'K': //Kilometers -> default
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
            }

            return dist;
        }

        private List<List<string>> GetClinics()
        {
            List<List<string>> result = new List<List<string>>();
            try
            {   // Open the text file using a stream reader.
                List<string> info;
                using (StreamReader sr = new StreamReader("clinics.txt"))
                {
                    // Read the stream to a string, and write the string to the console.
                    String line = sr.ReadToEnd();
                    info = new List<string>(line.Split('|'));
                    Console.WriteLine(line);
                }
                result.Add(info);
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return result;

        }

    }
}
