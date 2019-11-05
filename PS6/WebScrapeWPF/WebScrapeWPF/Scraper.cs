using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace WebScrapeWPF
{
    class Scraper
    {
        public List<string> csEnrollments;
        public List<string> descriptions;
        public Scraper()
        {
            csEnrollments = new List<string>();
            descriptions = new List<string>();
        }

        /// <summary>
        /// Given a semester, year, and a list of classes (maybe empty)
        /// This method will scrape the class schedule page based on parameters.
        /// Will return a string to display to the user.
        /// </summary>
        /// <param name="semester"></param>
        /// <param name="year"></param>
        /// <param name="classes"></param>
        /// <returns></returns>
        public List<string> GetEnrollments(string semester, string year)
        {
            var driver = new ChromeDriver();
            WebDriverWait wait = new WebDriverWait(driver, System.TimeSpan.FromSeconds(10));
            driver.Navigate().GoToUrl(MakeURL(semester, year));

            List<IWebElement> enrollTable;
            List<IWebElement> allCoursesTable;


            // Get all course information
            IWebElement courseTemp = driver.FindElementByXPath("//*[@id=\"classDetailsTable\"]/tbody");
            allCoursesTable = courseTemp.FindElements(By.XPath("//*[@class='odd' or @class='even']")).ToList();

            // Get all credits of courses
            List<string> credits = new List<string>();
            foreach (IWebElement el in allCoursesTable)
            {
                List<IWebElement> temp = el.FindElements(By.TagName("td")).ToList();

                if (temp[3].Text.Equals("001") && !temp[4].Text.Equals("Special Topics") && !temp[4].Text.Equals("Seminar") && 
                    Convert.ToInt32(temp[2].Text) > 1000 && Convert.ToInt32(temp[2].Text) < 7000)
                {
                    credits.Add(temp[5].Text);
                }
            }

            var clickEnroll = driver.FindElementByLinkText("Seating availability for all CS classes");
            clickEnroll.Click();

            // Get all enrollment information
            IWebElement enrollTemp = driver.FindElementByXPath("//*[@id=\"seatingAvailabilityTable\"]/tbody");
            enrollTable = enrollTemp.FindElements(By.TagName("tr")).ToList();


            List<IWebElement> toRemove = new List<IWebElement>();

            //Filter enrollments to get rid of any sections other than 001 and classes with numbers <1000 and >7000
            foreach (IWebElement el in enrollTable)
            {
                List<IWebElement> temp = el.FindElements(By.TagName("td")).ToList();
                if (temp[3].Text.Equals("001") && !temp[4].Text.Equals("Special Topics") && !temp[4].Text.Contains("Seminar") && 
                    Convert.ToInt32(temp[2].Text) > 1000 && Convert.ToInt32(temp[2].Text) < 7000)
                {
                    continue;
                }

                toRemove.Add(el);
            }

            // Remove unnecessary elements
            enrollTable = enrollTable.Except(toRemove).ToList();

            for (int i = 0; i < enrollTable.Count; i++)
            {
                List<IWebElement> enrollVals = enrollTable[i].FindElements(By.TagName("td")).ToList();

                // Dept,number,credits,title,enrollment,semester,year
                string enrollment = enrollVals[1].Text + "," +
                    enrollVals[2].Text + "," +
                    credits[i] + "," +
                    enrollVals[4].Text + "," +
                    enrollVals[6].Text + "," +
                    semester + "," +
                    year;

                this.csEnrollments.Add(enrollment);
            }

            return csEnrollments;
        }

        public List<string> getDescriptions(string courses)
        {
            var driver = new ChromeDriver();
            WebDriverWait wait = new WebDriverWait(driver, System.TimeSpan.FromSeconds(10));
            driver.Navigate().GoToUrl("https://catalog.utah.edu");

            Thread.Sleep(1000);

            var clickCourse = driver.FindElementByXPath("//*[@id=\"top\"]/div/div[3]/div/nav/ul/li[3]/div");
            clickCourse.Click();

            //var searchElement = driver.FindElementById("Search");
            //var searchElement = driver.FindElementByXPath("//*[@id=\"kuali - catalog - main\"]/div/div[1]/div[1]/div");

            var searchElement = driver.FindElementByXPath("//*[@id=\"Search\"]");
            List<string> sepCourses = SeparateClasses(courses);

            foreach (string course in sepCourses)
            {
                searchElement.SendKeys(course);
                //searchElement.SendKeys(Keys.Return);
                Thread.Sleep(2000);
                //var a = driver.FindElementByXPath("//*[@id=\"kuali - catalog - main\"]/div/div[1]/div[3]/button");
               // a.Click();
                //var clickDesc = driver.FindElementByLinkText(course);
                var clickDesc = driver.FindElementByXPath("//*[@id=\"__KUALI_TLP\"]/div/table/tbody/tr/th");

                clickDesc.Click();

                Thread.Sleep(3000);
                var desc = driver.FindElementByXPath("//*[@id=\"__KUALI_TLP\"]/div/div/div[4]/div");
                descriptions.Add(course + "," + desc.Text);
            }
            

            return descriptions;
        }

        /// <summary>
        /// Returns a string url corresponding to the semester and year given
        /// </summary>
        /// <param name="semester"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public string MakeURL(string semester, string year)
        {
            string yearURL = year.Substring(2, 2);
            string semURL = "";
            switch (semester)
            {
                case "Spring":
                    semURL = "4";
                    break;
                case "Summer":
                    semURL = "6";
                    break;
                case "Fall":
                    semURL = "8";
                    break;
            }

            return "https://student.apps.utah.edu/uofu/stu/ClassSchedules/main/1" + yearURL + semURL + "/class_list.html?subject=CS";
        }
        /// <summary>
        /// Given a string of comma separated classes, convert that into a more usable string array.
        /// </summary>
        /// <param name="classes"></param>
        /// <returns></returns>
        public List<string> SeparateClasses(string classes)
        {
            return classes.Split(',').Select(s => s.Trim()).ToList();
        }

    }
}
