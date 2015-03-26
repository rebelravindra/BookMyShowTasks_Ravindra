using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace BookMyShowTasks_Ravindra
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
            placebox.SelectionChanged += placebox_SelectionChanged_1;
            
            
        }
        public double Latitude,Longitude;
        

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
           // await ApplicationData.Current.ClearAsync();
            
            mapgrid.IsHitTestVisible = false;
            pbar.IsIndeterminate = true;
            try
            {
                blk.Text = "Getting Location...";
                Geolocator loc = new Geolocator();
                Geoposition pos = await loc.GetGeopositionAsync();
                Latitude = pos.Coordinate.Latitude;             //getting user's position
                Longitude = pos.Coordinate.Longitude;
                BasicGeoposition bpos = new BasicGeoposition();
                bpos.Latitude = Latitude;
                bpos.Longitude = Longitude;


                Geopoint point = new Geopoint(bpos);
                await mymap.TrySetViewAsync(point, 17);
                pbar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                MapIcon m = new MapIcon();
                m.Title = "You're here";            //loading pushpin to show user position
                m.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/pushpin.png"));
                m.Location = point;
                mymap.MapElements.Add(m);
                blk.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                
            }
            catch (Exception ex)
            {
                pbar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                MessageDialog m = new MessageDialog("Couldn't get your position, Please try later");
                m.ShowAsync();
            }
        }
     
      public List<BitmapImage> imageslist = new List<BitmapImage>();

      private async void placebox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
      {
          try
          {
              blk.Visibility = Windows.UI.Xaml.Visibility.Visible;
              blk.Text = "Getting places...";
              pbar.Visibility = Windows.UI.Xaml.Visibility.Visible;

              List<FinalData> myfinaldata = new List<FinalData>();
              pbar.IsIndeterminate = true;
              ComboBoxItem c = placebox.SelectedValue as ComboBoxItem;
              string s = c.Content.ToString();          
              s = s.ToLower();  //getting the selected item
              HttpClient client = new HttpClient();
              Uri u = new Uri("https://maps.googleapis.com/maps/api/place/search/json?types=" + s + "&location=" + Latitude + "," + Longitude + "&radius=5000&sensor=false&key=AIzaSyBTptqaDtNTB0Fmba3N98oWrucR0vuctRU");
              string jsondata = await client.GetStringAsync(u);
              RootObject roj = JsonConvert.DeserializeObject<RootObject>(jsondata); // Deserializing according to classes

              List<ImageData> myprlist = new List<ImageData>(); //ImageData class is just to seperate Photoreferences from actual response
              for (int i = 0; i < roj.results.Count; i++)
              {
                  var x = roj.results[i].photos;
                  if (x != null)

                      myprlist.Add(new ImageData { photo_reference = roj.results[i].photos[0].photo_reference });
                  else
                      myprlist.Add(new ImageData { photo_reference = "CnRvAAAAwMpdHeWlXl - lH0vp7lez4znKPIWSWvgvZFISdKx45AwJVP1Qp37YOrH7sqHMJ8C - vBDC546decipPHchJhHZL94RcTUfPa1jWzo - rSHaTlbNtjh - N68RkcToUCuY9v2HNpo5mziqkir37WU8FJEqVBIQ4k938TI3e7bf8xq - uwDZcxoUbO_ZJzPxremiQurAYzCTwRhE_V0" });
                  //took a place holder Reference value, just for now
              }


              for (int i = 0; i < myprlist.Count; i++)
              {
                  u = new Uri("https://maps.googleapis.com/maps/api/place/photo?maxwidth=400&photoreference=" + myprlist[i].photo_reference + "&key=AIzaSyBTptqaDtNTB0Fmba3N98oWrucR0vuctRU");
                  StorageFile f = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(Guid.NewGuid().ToString() + ".jpg");
                  IBuffer resp = await client.GetBufferAsync(u);
                  await FileIO.WriteBufferAsync(f, resp);  //Downloading images as buffer and storing in local folder
                  myfinaldata.Add(new FinalData { Name = roj.results[i].name, Path = f.Path,Address=roj.results[i].vicinity });
              }

              dataview.ItemsSource = myfinaldata;

              pbar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
              blk.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
              blk.Text = roj.results.Count == 0 ? "Nothing to display" : "";


          }
          catch (Exception ex)
          {
              MessageDialog m = new MessageDialog("problem loading data, pls try later");
              m.ShowAsync();
          }
      }

        private void dataview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Frame.Navigate(typeof(Placeinfo),dataview.SelectedItem);
        }
    }
}
