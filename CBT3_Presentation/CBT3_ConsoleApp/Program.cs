//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author: Scott Green
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Net;
using OpenTelemetry.Metrics;
using OpenTelemetry;
using System.Speech.Synthesis;
using System.Globalization;


namespace CBT3_ConsoleApp
{


    class Program
    {

        static async Task Main(string[] args)
        {
            CBT3_ServiceProvider.Initialize();
            
            while (true)
            {
                CBT3_EventHandlers.SubscribeToEvents();
                await CBT3_Play();
                CBT3_EventHandlers.UnsubscribeToEvents();
            }


        }
        static void ListInstalledVoices()
        {
            using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
            {
                Console.WriteLine("Installed voices:");
                foreach (var voice in synthesizer.GetInstalledVoices())
                {
                    Console.WriteLine($" - {voice.VoiceInfo.Name}");
                }
            }
        }
        static void ReadTextAloud(string text, string voiceName = null, int rate = 0, int volume = 100)
        {
            using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
            {
                synthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Child, 0, CultureInfo.GetCultureInfo("fr-FR")); // For French
                try
                {
                    if (!string.IsNullOrEmpty(voiceName))
                    {
                        synthesizer.SelectVoice(voiceName);
                    }

                    synthesizer.Rate = rate; // Range: -10 to 10
                    synthesizer.Volume = volume; // Range: 0 to 100
                    synthesizer.SelectVoice("Microsoft Zira Desktop");
                    
                    synthesizer.Speak(text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }
        private static async Task CBT3_Play()
        {
            
            CBT3_App.InitializeApp();

            /// <summary>
            /// First Name 
            /// </summary>
            CBT3_App.Trainee.FirstName = CBT3_App.GetFirstNameAsync().Result.Value;

            /// <summary>
            /// Last Name 
            /// </summary>
            CBT3_App.Trainee.LastName = CBT3_App.GetLastNameAsync().Result.Value;

            CBT3_ConsoleHelper.EraseLine();

            /// <summary>
            /// First and Second UPID 
            /// </summary>
            CBT3_App.Trainee.UPID = CBT3_App.GetUPIDAsync().Result.Value; ;

            CBT3_ConsoleHelper.EraseLine();

            /// <summary>
            /// First and Second Year Of Birth 
            /// </summary>
            CBT3_App.Trainee.YearOfBirth = CBT3_App.GetYearOfBirthAsync().Result.Value;

            CBT3_ConsoleHelper.EraseLine();

            var add_trainee = CBT3_App.AddTraineeAsync(CBT3_App.Trainee);

            if (add_trainee.Result.IsSuccess)
            {
                CBT3_App.Trainee = add_trainee.Result.Value;

                Console.WriteLine($"Registration complete => {CBT3_App.Trainee.ToString()}");
                Console.ReadLine();
                AnsiConsole.Clear();

            }
            else
            {
                Console.WriteLine($"{add_trainee.Result.Error.Message}");
            }

            /// <summary>
            /// GET COURSES (LITE) CQRS / MEDIATOR
            /// </summary>
            CBT3_ConsoleHelper.EraseLine();
            await CBT3_App.SelectCourseAsync();

            /// <summary>
            /// Get the fully loaded Course CQRS / MEDIATOR
            /// </summary>
            CBT3_ConsoleHelper.EraseLine();
            await CBT3_App.StartCourseAsync();
            await CBT3_App.FinishCourseAsync();
            bool done = true;
            while(done)
            {
                CBT3_ConsoleHelper.EraseLine();
                done = CBT3_App.SelectAnotherCourse().Value;
                break;
            }
            
        }












    }


}
