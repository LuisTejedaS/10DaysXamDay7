using SQLite;
using System;
using System.Collections.Generic;
using TenXamarinD7.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TenXamarinD7
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ExperienceView : ContentPage
	{
        List<Experience> experiencesItems = new List<Experience>();

        public ExperienceView()
        {
            InitializeComponent();
            BindingContext = experiencesItems;
        }


        void Handle_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new MainPage());

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ReadExperiences();
        }

        private void ReadExperiences()
        {
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabasePath))
            {
                conn.CreateTable<Experience>();
                List<Experience> experiences = conn.Table<Experience>().ToList();
                experiencesListView.ItemsSource = experiences;
            }
        }
    }
}