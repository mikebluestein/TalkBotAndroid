using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Speech.Tts;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TalkBot
{
    [Activity (Label = "Talk Bot", MainLauncher = true, Theme = "@android:style/Theme.Holo.Light", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity, TextToSpeech.IOnInitListener
    {
        TextToSpeech speech;
        EditText textToSpeak;
        Button speechButton;
        ListView speechItemListView;
        ArrayAdapter<string> adapter;
        List<string> items;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.Main);

            textToSpeak = FindViewById<EditText> (Resource.Id.textToSpeak);
            speechButton = FindViewById<Button> (Resource.Id.speechButton);
            speechItemListView = FindViewById<ListView> (Resource.Id.speechItemListView);

            speech = new TextToSpeech (this, this);
            speech.SetLanguage (Java.Util.Locale.Us);

            speechButton.Click += (object sender, EventArgs e) => {

                string text = textToSpeak.Text;

                if (!String.IsNullOrEmpty (text)) {
                    speech.Speak (text, QueueMode.Add, null);
                } else {
                    Toast.MakeText (this, "Please enter some text to speak", ToastLength.Short).Show ();
                }
            };

            speechItemListView.ChoiceMode = ChoiceMode.Single;

            if (speechItemListView != null) {           
                speechItemListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
                    textToSpeak.Text = items [e.Position];
                    speechItemListView.SetSelection (e.Position);
                };
            }
        }

        public override bool OnCreateOptionsMenu (IMenu menu)
        {
            menu.Add (0, 0, 0, "Save Item");
            menu.Add (0, 1, 1, "Delete Item");
            return true;
        }

        public override bool OnOptionsItemSelected (IMenuItem item)
        {
            switch (item.ItemId) {
            case 0:
                items.Add (textToSpeak.Text);
                adapter.Add (textToSpeak.Text);
                return true;
            case 1:
                int i = speechItemListView.CheckedItemPosition;
                if (i >= 0) {
                    items.RemoveAt (i);
                    adapter.Clear ();
                    adapter.AddAll (items);
                    speechItemListView.SetItemChecked (-1, true);
                } else {
                    Toast.MakeText (this, "Please select an item to delete", ToastLength.Short).Show ();
                }
                return true;
            default:
                return base.OnOptionsItemSelected (item);
            }
        }

        protected override void OnResume ()
        {
            base.OnResume ();

            items = Util.ReadFromDisk () ?? new List<string> {
                "Please",
                "Thank You",
                "You're welcome",
                "$123.45",
                "Hello, how are you?",
                "1+1=2",
                "10%"
            };

            adapter = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleListItemChecked, items);
            speechItemListView.Adapter = adapter;
        }

        protected override void OnPause ()
        {
            base.OnPause ();

            items.SaveToDisk ();
        }

        public void OnInit (OperationResult status)
        {
        }
    }

    public static class Util
    {
        public static void SaveToDisk (this List<string> speechItems)
        {
            using (Stream s = File.Open (SpeechItemsPath (), FileMode.Create)) {
                var bf = new BinaryFormatter ();
                bf.Serialize (s, speechItems);
            }
        }

        public static string SpeechItemsPath ()
        {
            string documentsDir = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
            string speechItemsPath = Path.Combine (documentsDir, "speechitems.bin");

            return speechItemsPath;
        }

        public static List<string> ReadFromDisk ()
        {
            List<string> items = null;

            string path = SpeechItemsPath ();

            if (File.Exists (path)) {
                using (Stream stream = File.Open (path, FileMode.Open)) {
                    var bf = new BinaryFormatter ();
                    items = (List<string>)bf.Deserialize (stream);
                }
            }
            return items;
        }
    }
}