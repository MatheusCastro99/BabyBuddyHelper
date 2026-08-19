using System.Diagnostics;

namespace BabyBuddyHelper
{
    public partial class MainPage : ContentPage
{
        DateTime expectedDueDate = new(2027, 02, 02);
        DateTime currentDate;
        bool isBabyBorn;

        public int MonthsUntilDue { get; private set; }
        public int DaysUntilDue { get; private set; }
        public string CountdownText { get; private set; } = "";

        public MainPage()
        {
            InitializeComponent();

            HandleCounter(); //Starts counter
            
            BindingContext = this;
        }

        private void HandleCounter()
        {
            currentDate = DateTime.Now;

            if (currentDate >= expectedDueDate) //Handles dates after specified due date
            {
                isBabyBorn = true;

                // Calculate months and days since birth
                MonthsUntilDue = (currentDate.Year - expectedDueDate.Year) * 12 + (currentDate.Month - expectedDueDate.Month);
                DaysUntilDue = currentDate.Day - expectedDueDate.Day;

                // Handle negative days
                if (DaysUntilDue < 0)
                {
                    MonthsUntilDue--;
                    DaysUntilDue += DateTime.DaysInMonth(expectedDueDate.Year, expectedDueDate.Month);
                }

                CountdownText = $"Welcome Baby! You are {MonthsUntilDue} Months and {DaysUntilDue} Days old";
            }

            else // Handles dates before due date
            {
                isBabyBorn = false;

                // Calculate months and remaining days
                MonthsUntilDue = (expectedDueDate.Year - currentDate.Year) * 12 + (expectedDueDate.Month - currentDate.Month);
                DaysUntilDue = expectedDueDate.Day - currentDate.Day;

                // Handle negative days by adjusting months
                if (DaysUntilDue < 0)
                {
                    MonthsUntilDue--;
                    DaysUntilDue += DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                }

                // Create formatted text
                CountdownText = $"{MonthsUntilDue} Months and {DaysUntilDue} Days until new Baby drops in!";
            }
        }
    }
}
