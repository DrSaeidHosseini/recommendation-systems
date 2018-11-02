/*
---------------------------------------------
Copyright, Dr. Saeid Hosseini

https://sites.google.com/view/dr-seyed-saeid-hosseini/home
https://scholar.google.com/citations?hl=en&user=Q3DhDl0AAAAJ
http://people.sutd.edu.sg/~hosseini_saeid/

---------------------------------------------
Reference: Cite any of relevant articles as follows:

--Hosseini, Saeid, et al. "Leveraging multi-aspect time-related influence in location recommendation." World Wide Web (2017): 1-28.
--Hosseini, Saeid. "Location Inference and Recommendation in Social Networks." (2017).

--Hosseini, Saeid, and Lei Thor Li. "Point-of-interest recommendation using temporal orientations of users and locations." International Conference on Database Systems for Advanced Applications. Springer, Cham, 2016.
--Hosseini, Saeid, et al. "Jointly modeling heterogeneous temporal properties in location recommendation." International Conference on Database Systems for Advanced Applications. Springer, Cham, 2017.
---------------------------------------------
*/
/*
Temporal slabs are blocks of similar temporal slots (e.g. 1am - 4am in hour dimension or Sat. and Sun. in day dimension). Here is how we extract temporal slabs.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class algorithms_Data_Temporal_Slots : System.Web.UI.Page
{
    private class UTP_Matrix_Item
    {
        private Int64 m_userID;
        private string m_VenueId;
        private string m_VenueName;
        private int m_datePart1;

        public UTP_Matrix_Item()
        {
        }
        public UTP_Matrix_Item(Int64 t_userID, string t_VenueId, string t_VenueName, int t_datePart1)
        {
            m_userID = t_userID;
            m_VenueId = t_VenueId;
            m_VenueName = t_VenueName;
            m_datePart1 = t_datePart1;
        }
        public Int64 userID
        {
            get { return m_userID; }
            set { m_userID = value; }
        }
        public string VenueId
        {
            get { return m_VenueId; }
            set { m_VenueId = value; }
        }
        public string VenueName
        {
            get { return m_VenueName; }
            set { m_VenueName = value; }
        }
        public int datePart1
        {
            get { return m_datePart1; }
            set { m_datePart1 = value; }
        }
    }

    private class TimeSlotSimilarity
    {
        public Int64 userID { get; set; }
        public int TimeSlot1 { get; set; }
        public int TimeSlot2 { get; set; }
        public double CosineSim { get; set; }
    }

    private class TimeSlotSimilarity_Float
    {
        public Int64 userID { get; set; }
        public int TimeSlot1 { get; set; }
        public int TimeSlot2 { get; set; }
        public float CosineSim { get; set; }
    }

    protected void Get_Temporal_Slabs(object sender, EventArgs e)
    {
        Server.ScriptTimeout = 300000;
        /*
        Description:
         Find the similarity between two time slots –Then Merge similar slot to form the time cluser
         Find the Clusters of the day time and weekday: Zt and Zd which are the latent variables
         We find the similarity between day time clusters (e.g. hours of the day) and merge them together to detect a few clusters for the daytime activity.         
        */

        //Compute the similarity between hour time slots.
        Data_Similarity_Daytime_Method();

        //We want to see a matrix which is h*h. h is 24 which is the hourly time slots.
        //Generate the matrix which is used for Cluster 3.0 program.
        //HoursSlotSimilarityMatrix();

        /*
         1	Sunday
         2	Monday
         3	Tuesday
         4	Wednesday
         5	Thursday
         6	Friday
         7	Saturday         
        */

        //Compute the similarity between day of the week time slots.
        Data_Similarity_DayOfWeek_Method();        
        WeekDaysSlotSimilarityMatrix();        

        //Compute the similarity between week of the month time slots.
        Data_Similarity_WeekOfMonth_Method();
        MonthWeeksSlotSimilarityMatrix();

        //Compute the similarity between month of the year time slots.
        Data_Similarity_MonthOfYear_Method();
        YearMonthsSlotSimilarityMatrix();
    }

    private void YearMonthsSlotSimilarityMatrix()
    {
        DataTable dt1 = MonthSlots_Similarity();
        Response.Write("Mt\t");
        for (int i = 1; i < 13; i++)
            Response.Write("M" + i.ToString() + "\t");
        Response.Write("<br/>");
        Debug.WriteLine(DateTime.Now.ToString());
        for (int i = 1; i < 13; i++)
        {
            Response.Write("M" + i.ToString() + "\t");
            for (int j = 1; j < 13; j++)
            {
                if (i == j)
                    Response.Write("1.000");
                else
                {
                    try
                    {
                        DataRow[] result = dt1.Select("((Month1=" + i.ToString() + " and Month2=" + j.ToString() + ") or (Month2=" + i.ToString() + " and Month1=" + j.ToString() + "))");
                        if (result != null)
                        {
                            //Response.Write("(" + i.ToString() + "," + j.ToString() + ")=" + General.StringValue(result[0][2]));
                            if ((int)result[0][3] < 2) //Check the number of instances for the similarity value. Number of evidences for the similarity between two time slots.
                                Response.Write(General.StringValue(result[0][2]));
                            else
                                Response.Write(General.StringValue(result[0][2]));
                        }
                        else
                            //Response.Write("(" + i.ToString() + "," + j.ToString() + ")=" + General.StringValue("-1"));
                            Response.Write(General.StringValue("0.888"));
                    }
                    catch (Exception ex)
                    {
                        //Response.Write("(" + i.ToString() + "," + j.ToString() + ")=-1");
                        Response.Write("0.888");
                    }
                }
                Response.Write("\t");
            }
            Response.Write("<br/>");
        }
        Response.End();
        Debug.WriteLine(DateTime.Now.ToString());
        Response.End();
    }
    private void MonthWeeksSlotSimilarityMatrix()
    {
        DataTable dt1 = WeekSlots_Similarity();
        Response.Write("Wt\t");
        for (int i = 1; i < 6; i++)
            Response.Write("W" + i.ToString() + "\t");
        Response.Write("<br/>");
        Debug.WriteLine(DateTime.Now.ToString());
        for (int i = 1; i < 6; i++)
        {
            Response.Write("W" + i.ToString() + "\t");
            for (int j = 1; j < 6; j++)
            {
                if (i == j)
                    Response.Write("1.000");
                else
                {
                    try
                    {
                        DataRow[] result = dt1.Select("((Week1=" + i.ToString() + " and Week2=" + j.ToString() + ") or (Week2=" + i.ToString() + " and Week1=" + j.ToString() + "))");
                        if (result != null)
                        {
                            //Response.Write("(" + i.ToString() + "," + j.ToString() + ")=" + General.StringValue(result[0][2]));
                            if ((int)result[0][3] < 2) //Check the number of instances for the similarity value. Number of evidences for the similarity between two time slots.
                                Response.Write(General.StringValue(result[0][2]));
                            else
                                Response.Write(General.StringValue(result[0][2]));
                        }
                        else
                            //Response.Write("(" + i.ToString() + "," + j.ToString() + ")=" + General.StringValue("-1"));
                            Response.Write(General.StringValue("0.888"));
                    }
                    catch (Exception ex)
                    {
                        //Response.Write("(" + i.ToString() + "," + j.ToString() + ")=-1");
                        Response.Write("0.888");
                    }
                }
                Response.Write("\t");
            }
            Response.Write("<br/>");
        }
        Response.End();
        Debug.WriteLine(DateTime.Now.ToString());
        Response.End();
    }
    private void WeekDaysSlotSimilarityMatrix()
    {
        DataTable dt1 = DaySlots_Similarity();
        Response.Write("Dt\t");
        for (int i = 1; i < 8; i++)
            Response.Write("D" + i.ToString() + "\t");
        Response.Write("<br/>");
        Debug.WriteLine(DateTime.Now.ToString());
        for (int i = 1; i < 8; i++)
        {
            Response.Write("D" + i.ToString() + "\t");
            for (int j = 1; j < 8; j++)
            {
                if (i == j)
                    Response.Write("1.000");
                else
                {
                    try
                    {
                        DataRow[] result = dt1.Select("((Day1=" + i.ToString() + " and Day2=" + j.ToString() + ") or (Day2=" + i.ToString() + " and Day1=" + j.ToString() + "))");
                        if (result != null)
                        {
                            //Response.Write("(" + i.ToString() + "," + j.ToString() + ")=" + General.StringValue(result[0][2]));
                            if ((int)result[0][3] < 2) //Check the number of instances for the similarity value. Number of evidences for the similarity between two time slots.
                                Response.Write(General.StringValue(result[0][2]));
                            else
                                Response.Write(General.StringValue(result[0][2]));
                        }
                        else
                            //Response.Write("(" + i.ToString() + "," + j.ToString() + ")=" + General.StringValue("-1"));
                            Response.Write(General.StringValue("0.888"));
                    }
                    catch (Exception ex)
                    {
                        //Response.Write("(" + i.ToString() + "," + j.ToString() + ")=-1");
                        Response.Write("0.888");
                    }
                }
                Response.Write("\t");
            }
            Response.Write("<br/>");
        }
        Response.End();
        Debug.WriteLine(DateTime.Now.ToString());
        Response.End();
    }
    private void HoursSlotSimilarityMatrix()
    {
        DataTable dt1 = HourSlots_Similarity();
        Response.Write("Dt\t");
        for (int i = 0; i < 25; i++)
            Response.Write("H" + i.ToString() + "\t");
        Response.Write("<br/>");
        Debug.WriteLine(DateTime.Now.ToString());
        for (int i = 0; i < 24; i++)
        {
            Response.Write("H" + i.ToString() + "\t");
            for (int j = 0; j < 24; j++)
            {
                if (i == j)
                    Response.Write("1.000");
                else
                {
                    try
                    {
                        DataRow[] result = dt1.Select("((Hour1=" + i.ToString() + " and Hour2=" + j.ToString() + ") or (Hour2=" + i.ToString() + " and Hour1=" + j.ToString() + "))");
                        if (result != null)
                        {
                            //Response.Write("(" + i.ToString() + "," + j.ToString() + ")=" + General.StringValue(result[0][2]));
                            if ((int)result[0][3]==2) //Check the number of instances for the similarity value. Number of evidences for the similarity between two time slots.
                                Response.Write(General.StringValue("T") + General.StringValue(result[0][2]));
                            else
                                if ((int)result[0][3] == 1) //Check the number of instances for the similarity value. Number of evidences for the similarity between two time slots.
                                    Response.Write(General.StringValue("N") + General.StringValue(result[0][2]));
                            else
                                Response.Write(General.StringValue(result[0][2]));
                        }
                        else
                            //Response.Write("(" + i.ToString() + "," + j.ToString() + ")=" + General.StringValue("-1"));
                            Response.Write(General.StringValue("0.888"));
                    }
                    catch (Exception ex)
                    {
                        //Response.Write("(" + i.ToString() + "," + j.ToString() + ")=-1");
                        Response.Write("0.888");
                    }
                }
                Response.Write("\t");
            }
            Response.Write("<br/>");
        }
        Debug.WriteLine(DateTime.Now.ToString());
        Response.End();
    }
    private void Data_Similarity_Daytime_Method()
    {
        List<UTP_Matrix_Item> UTP_Matrix;

        Debug.WriteLine("Started: "+DateTime.Now.ToString());

        //UTP: User, Time of the day (hour), POI matrix
        //userID,VenueId,VenueName,datePart1
        DataTable dt1 = Data_TestUsers_Venue_TimeOfDay_UTP();
        //Create UTP matrix based on datatable
        UTP_Matrix = dt1.AsEnumerable()
        .Select(row => new UTP_Matrix_Item
        {
            userID = row.Field<Int64>("user"),
            VenueId = row.Field<string>("location_id"),
            VenueName = "",//row.Field<string>("VenueName"),
            datePart1 = row.Field<int>("datePart1")
        }).ToList();

        int iCounter = 0;
        //foreach (var t in UTP_Matrix)
        //{
        //    iCounter++;
        //    if (iCounter > 50)
        //        continue;
        //    Response.Write("<br/>" + t.userID.ToString() + "-------" + t.VenueId.ToString() + "-------" + t.VenueName + "-------"+t.datePart1.ToString());
        //}

        //Get the list of unique users
        var UniqueUsers= UTP_Matrix.GroupBy(x => x.userID).Select(x => x.First());

        iCounter = 0;

        List<TimeSlotSimilarity> List_Similarity = new List<TimeSlotSimilarity>();

        //Iterate through all the users
        foreach (var t in UniqueUsers)
        {
            iCounter++;
            //Response.Write("<br/>"+iCounter.ToString()+". " + t.userID);

            //Get the UTP of the current user
            var TimeSlot_Locations = UTP_Matrix.Where(x => x.userID == t.userID).GroupBy(ac => new
            {
                ac.datePart1,
                ac.VenueId,
                ac.VenueName
            }).Select(x => x.First());
            

            //Get distinctive time slots that this user has visited.
            var UniqueTimeSlots = TimeSlot_Locations.OrderBy(x => x.datePart1).GroupBy(x => x.datePart1).Select(x => x.First());

            Debug.WriteLine(iCounter.ToString() + ". " + t.userID);

            //Compare each of time slots pairs and write down the similarity.
            foreach (var ts1 in UniqueTimeSlots)
                foreach (var ts2 in UniqueTimeSlots)
                    if (ts1.datePart1 < ts2.datePart1)
                    {

                        //Get the itmes of timeslot pair to compare.
                        var TimeSlot_1 = TimeSlot_Locations.Where(x => x.datePart1 == ts1.datePart1);
                        var TimeSlot_2 = TimeSlot_Locations.Where(x => x.datePart1 == ts2.datePart1);

                        //Continue from here.
                        var shared_items = TimeSlot_1.Where(c => TimeSlot_2.All(w => w.VenueId == c.VenueId));                        
                        if (shared_items.Count() > 0)
                        {
                            //Debug.WriteLine(" --> (" + ts1.datePart1 + "," + ts2.datePart1 + ")=" + shared_items.Count());
                            //Items that two time-slot share
                            //foreach (var si in shared_items)
                            //    Debug.WriteLine("<br/>" + si.userID + ", " + si.VenueId + ", " + si.VenueName + ", " + si.datePart1);
                            //Similarity: Divide the number of shared items on sqrt(Timeslot1.numberof items)*sqrt(Timeslot2.numberofitems)
                            //Debug.WriteLine(shared_items.Count() + "--" + TimeSlot_1.Count() + ">" + Math.Sqrt(TimeSlot_1.Count()) + "---" + TimeSlot_2.Count() + ">" + Math.Sqrt(TimeSlot_2.Count()));
                            //Debug.WriteLine;
                            Debug.WriteLine("Similarity(" + ts1.datePart1 + "," + ts2.datePart1 + ")=" + (shared_items.Count() / (Math.Sqrt(TimeSlot_1.Count())) * (Math.Sqrt(TimeSlot_2.Count()))));
                            List_Similarity.Add(new TimeSlotSimilarity { userID = t.userID, TimeSlot1 = ts1.datePart1, TimeSlot2 = ts2.datePart1, CosineSim=(shared_items.Count() / (Math.Sqrt(TimeSlot_1.Count())) * (Math.Sqrt(TimeSlot_2.Count()))) });
                        }
                        //var IntersectionBet_TimeSlots = TimeSlot_1.Where(w => TimeSlot_2.Select(s2 => s2.VenueId).Contains(w.ven)).ToList();
                    }
            //if (iCounter > 1)
            //   break;
        }
        var List_SimilarityOrdered = List_Similarity.OrderBy(x => x.TimeSlot1).ThenBy(x => x.TimeSlot2).ToList();

        foreach (var simItem in List_SimilarityOrdered)
            Debug.WriteLine(simItem.userID.ToString() + "--> Sim(" + simItem.TimeSlot1 + "," + simItem.TimeSlot2 + ")=" + simItem.CosineSim);

        Debug.WriteLine("Finished: " + DateTime.Now.ToString());

        
    }

    private void Data_Similarity_DayOfWeek_Method()
    {
        List<UTP_Matrix_Item> UTP_Matrix;

        Debug.WriteLine("Started: " + DateTime.Now.ToString());

        //UTP: User, Day of week, POI matrix
        //userID,VenueId,VenueName,datePart1: Day in the week
        DataTable dt1 = Data_TestUsers_Venue_DayOfWeek_UTP();
        //Create UTP matrix based on datatable
        UTP_Matrix = dt1.AsEnumerable()
        .Select(row => new UTP_Matrix_Item
        {
            userID = row.Field<Int64>("user"),
            VenueId = row.Field<string>("location_id"),
            VenueName = "",//row.Field<string>("VenueName"),
            datePart1 = row.Field<int>("datePart1")
        }).ToList();

        int iCounter = 0;
        //foreach (var t in UTP_Matrix)
        //{
        //    iCounter++;
        //    if (iCounter > 50)
        //        continue;
        //    Response.Write("<br/>" + t.userID.ToString() + "-------" + t.VenueId.ToString() + "-------" + t.VenueName + "-------"+t.datePart1.ToString());
        //}

        //Get the list of unique users
        var UniqueUsers = UTP_Matrix.GroupBy(x => x.userID).Select(x => x.First());

        iCounter = 0;

        List<TimeSlotSimilarity> List_Similarity = new List<TimeSlotSimilarity>();

        //Iterate through all the users
        foreach (var t in UniqueUsers)
        {
            iCounter++;
            //Response.Write("<br/>"+iCounter.ToString()+". " + t.userID);

            //Get the UTP of the current user
            var TimeSlot_Locations = UTP_Matrix.Where(x => x.userID == t.userID).GroupBy(ac => new
            {
                ac.datePart1,
                ac.VenueId,
                ac.VenueName
            }).Select(x => x.First());

            //Get distinctive time slots that this user has visited.
            var UniqueTimeSlots = TimeSlot_Locations.OrderBy(x => x.datePart1).GroupBy(x => x.datePart1).Select(x => x.First());

            Debug.WriteLine(iCounter.ToString() + ". " + t.userID);

            //Compare each of time slots pairs and write down the similarity.
            foreach (var ts1 in UniqueTimeSlots)
                foreach (var ts2 in UniqueTimeSlots)
                    if (ts1.datePart1 < ts2.datePart1)
                    {
                        //Get the itmes of timeslot pair to compare.
                        var TimeSlot_1 = TimeSlot_Locations.Where(x => x.datePart1 == ts1.datePart1);
                        var TimeSlot_2 = TimeSlot_Locations.Where(x => x.datePart1 == ts2.datePart1);

                        //Continue from here.
                        var shared_items = TimeSlot_1.Where(c => TimeSlot_2.All(w => w.VenueId == c.VenueId));
                        if (shared_items.Count() > 0)
                        {
                            //Debug.WriteLine(" --> (" + ts1.datePart1 + "," + ts2.datePart1 + ")=" + shared_items.Count());
                            //Items that two time-slot share
                            //foreach (var si in shared_items)
                            //    Debug.WriteLine("<br/>" + si.userID + ", " + si.VenueId + ", " + si.VenueName + ", " + si.datePart1);
                            //Similarity: Divide the number of shared items on sqrt(Timeslot1.numberof items)*sqrt(Timeslot2.numberofitems)
                            //Debug.WriteLine(shared_items.Count() + "--" + TimeSlot_1.Count() + ">" + Math.Sqrt(TimeSlot_1.Count()) + "---" + TimeSlot_2.Count() + ">" + Math.Sqrt(TimeSlot_2.Count()));
                            //Debug.WriteLine;
                            Debug.WriteLine("Similarity(" + ts1.datePart1 + "," + ts2.datePart1 + ")=" + (shared_items.Count() / (Math.Sqrt(TimeSlot_1.Count())) * (Math.Sqrt(TimeSlot_2.Count()))));
                            List_Similarity.Add(new TimeSlotSimilarity { userID = t.userID, TimeSlot1 = ts1.datePart1, TimeSlot2 = ts2.datePart1, CosineSim = (shared_items.Count() / (Math.Sqrt(TimeSlot_1.Count())) * (Math.Sqrt(TimeSlot_2.Count()))) });
                        }
                        //var IntersectionBet_TimeSlots = TimeSlot_1.Where(w => TimeSlot_2.Select(s2 => s2.VenueId).Contains(w.ven)).ToList();
                    }
            //if (iCounter > 1)
            //   break;
        }
        var List_SimilarityOrdered = List_Similarity.OrderBy(x => x.TimeSlot1).ThenBy(x => x.TimeSlot2).ToList();

        foreach (var simItem in List_SimilarityOrdered)
        {
            Debug.WriteLine(simItem.userID.ToString() + "--> Sim(" + simItem.TimeSlot1 + "," + simItem.TimeSlot2 + ")=" + simItem.CosineSim);

        }
    }

    private void Data_Similarity_WeekOfMonth_Method()
    {
        List<UTP_Matrix_Item> UTP_Matrix;

        Debug.WriteLine("Started: " + DateTime.Now.ToString());

        //UTP: User, Week of month, POI matrix
        //userID,VenueId,VenueName,datePart1: Week in the month
        DataTable dt1 = Data_TestUsers_Venue_WeekOfMonth_UTP();
        
        //Create UTP matrix based on datatable
        UTP_Matrix = dt1.AsEnumerable()
        .Select(row => new UTP_Matrix_Item
        {
            userID = row.Field<Int64>("user"),
            VenueId = row.Field<string>("location_id"),
            VenueName = "",//row.Field<string>("VenueName"),
            datePart1 = row.Field<int>("datePart1")
        }).ToList();

        int iCounter = 0;
        //foreach (var t in UTP_Matrix)
        //{
        //    iCounter++;
        //    if (iCounter > 50)
        //        continue;
        //    Response.Write("<br/>" + t.userID.ToString() + "-------" + t.VenueId.ToString() + "-------" + t.VenueName + "-------"+t.datePart1.ToString());
        //}

        //Get the list of unique users
        var UniqueUsers = UTP_Matrix.GroupBy(x => x.userID).Select(x => x.First());

        iCounter = 0;

        List<TimeSlotSimilarity> List_Similarity = new List<TimeSlotSimilarity>();

        //Iterate through all the users
        foreach (var t in UniqueUsers)
        {
            iCounter++;
            //Response.Write("<br/>"+iCounter.ToString()+". " + t.userID);

            //Get the UTP of the current user
            var TimeSlot_Locations = UTP_Matrix.Where(x => x.userID == t.userID).GroupBy(ac => new
            {
                ac.datePart1,
                ac.VenueId,
                ac.VenueName
            }).Select(x => x.First());

            //Get distinctive time slots that this user has visited.
            var UniqueTimeSlots = TimeSlot_Locations.OrderBy(x => x.datePart1).GroupBy(x => x.datePart1).Select(x => x.First());

            Debug.WriteLine(iCounter.ToString() + ". " + t.userID);

            //Compare each of time slots pairs and write down the similarity.
            foreach (var ts1 in UniqueTimeSlots)
                foreach (var ts2 in UniqueTimeSlots)
                    if (ts1.datePart1 < ts2.datePart1)
                    {
                        //Get the itmes of timeslot pair to compare.
                        var TimeSlot_1 = TimeSlot_Locations.Where(x => x.datePart1 == ts1.datePart1);
                        var TimeSlot_2 = TimeSlot_Locations.Where(x => x.datePart1 == ts2.datePart1);

                        //Continue from here.
                        var shared_items = TimeSlot_1.Where(c => TimeSlot_2.All(w => w.VenueId == c.VenueId));
                        if (shared_items.Count() > 0)
                        {
                            //Debug.WriteLine(" --> (" + ts1.datePart1 + "," + ts2.datePart1 + ")=" + shared_items.Count());
                            //Items that two time-slot share
                            //foreach (var si in shared_items)
                            //    Debug.WriteLine("<br/>" + si.userID + ", " + si.VenueId + ", " + si.VenueName + ", " + si.datePart1);
                            //Similarity: Divide the number of shared items on sqrt(Timeslot1.numberof items)*sqrt(Timeslot2.numberofitems)
                            //Debug.WriteLine(shared_items.Count() + "--" + TimeSlot_1.Count() + ">" + Math.Sqrt(TimeSlot_1.Count()) + "---" + TimeSlot_2.Count() + ">" + Math.Sqrt(TimeSlot_2.Count()));
                            //Debug.WriteLine;
                            Debug.WriteLine("Similarity(" + ts1.datePart1 + "," + ts2.datePart1 + ")=" + (shared_items.Count() / (Math.Sqrt(TimeSlot_1.Count())) * (Math.Sqrt(TimeSlot_2.Count()))));
                            List_Similarity.Add(new TimeSlotSimilarity { userID = t.userID, TimeSlot1 = ts1.datePart1, TimeSlot2 = ts2.datePart1, CosineSim = (shared_items.Count() / (Math.Sqrt(TimeSlot_1.Count())) * (Math.Sqrt(TimeSlot_2.Count()))) });
                        }
                        //var IntersectionBet_TimeSlots = TimeSlot_1.Where(w => TimeSlot_2.Select(s2 => s2.VenueId).Contains(w.ven)).ToList();
                    }
            //if (iCounter > 30)
            //   break;
        }
        var List_SimilarityOrdered = List_Similarity.OrderBy(x => x.TimeSlot1).ThenBy(x => x.TimeSlot2).ToList();

        foreach (var simItem in List_SimilarityOrdered)
        {
            Debug.WriteLine(simItem.userID.ToString() + "--> Sim(" + simItem.TimeSlot1 + "," + simItem.TimeSlot2 + ")=" + simItem.CosineSim);

        }
        Debug.WriteLine("Finished Week of Month: " + DateTime.Now.ToString());        
        //Ok here is the reason.
        //Response.End();
    }
    private void Data_Similarity_MonthOfYear_Method()
    {
        List<UTP_Matrix_Item> UTP_Matrix;
        Debug.WriteLine("Started: " + DateTime.Now.ToString());

        //UTP: User, Month of Year, POI matrix
        //userID,VenueId,VenueName,datePart1: Month in the year
        DataTable dt1 = Data_TestUsers_Venue_MonthOfYear_UTP();

        //Create UTP matrix based on datatable
        UTP_Matrix = dt1.AsEnumerable()
        .Select(row => new UTP_Matrix_Item
        {
            userID = row.Field<Int64>("user"),
            VenueId = row.Field<string>("location_id"),
            VenueName = "",//row.Field<string>("VenueName"),
            datePart1 = row.Field<int>("datePart1")
        }).ToList();

        int iCounter = 0;
        //foreach (var t in UTP_Matrix)
        //{
        //    iCounter++;
        //    if (iCounter > 50)
        //        continue;
        //    Response.Write("<br/>" + t.userID.ToString() + "-------" + t.VenueId.ToString() + "-------" + t.VenueName + "-------"+t.datePart1.ToString());
        //}

        //Get the list of unique users
        var UniqueUsers = UTP_Matrix.GroupBy(x => x.userID).Select(x => x.First());

        iCounter = 0;

        List<TimeSlotSimilarity> List_Similarity = new List<TimeSlotSimilarity>();

        //Iterate through all the users
        foreach (var t in UniqueUsers)
        {
            iCounter++;
            //Response.Write("<br/>"+iCounter.ToString()+". " + t.userID);

            //Get the UTP of the current user
            var TimeSlot_Locations = UTP_Matrix.Where(x => x.userID == t.userID).GroupBy(ac => new
            {
                ac.datePart1,
                ac.VenueId,
                ac.VenueName
            }).Select(x => x.First());

            //Get distinctive time slots that this user has visited.
            var UniqueTimeSlots = TimeSlot_Locations.OrderBy(x => x.datePart1).GroupBy(x => x.datePart1).Select(x => x.First());

            Debug.WriteLine(iCounter.ToString() + ". " + t.userID);

            //Compare each of time slots pairs and write down the similarity.
            foreach (var ts1 in UniqueTimeSlots)
                foreach (var ts2 in UniqueTimeSlots)
                    if (ts1.datePart1 < ts2.datePart1)
                    {
                        //Get the itmes of timeslot pair to compare.
                        var TimeSlot_1 = TimeSlot_Locations.Where(x => x.datePart1 == ts1.datePart1);
                        var TimeSlot_2 = TimeSlot_Locations.Where(x => x.datePart1 == ts2.datePart1);

                        //Continue from here.
                        var shared_items = TimeSlot_1.Where(c => TimeSlot_2.All(w => w.VenueId == c.VenueId));
                        if (shared_items.Count() > 0)
                        {
                            //Debug.WriteLine(" --> (" + ts1.datePart1 + "," + ts2.datePart1 + ")=" + shared_items.Count());
                            //Items that two time-slot share
                            //foreach (var si in shared_items)
                            //    Debug.WriteLine("<br/>" + si.userID + ", " + si.VenueId + ", " + si.VenueName + ", " + si.datePart1);
                            //Similarity: Divide the number of shared items on sqrt(Timeslot1.numberof items)*sqrt(Timeslot2.numberofitems)
                            //Debug.WriteLine(shared_items.Count() + "--" + TimeSlot_1.Count() + ">" + Math.Sqrt(TimeSlot_1.Count()) + "---" + TimeSlot_2.Count() + ">" + Math.Sqrt(TimeSlot_2.Count()));
                            //Debug.WriteLine;
                            Debug.WriteLine("Similarity(" + ts1.datePart1 + "," + ts2.datePart1 + ")=" + (shared_items.Count() / (Math.Sqrt(TimeSlot_1.Count())) * (Math.Sqrt(TimeSlot_2.Count()))));
                            List_Similarity.Add(new TimeSlotSimilarity { userID = t.userID, TimeSlot1 = ts1.datePart1, TimeSlot2 = ts2.datePart1, CosineSim = (shared_items.Count() / (Math.Sqrt(TimeSlot_1.Count())) * (Math.Sqrt(TimeSlot_2.Count()))) });
                        }
                        //var IntersectionBet_TimeSlots = TimeSlot_1.Where(w => TimeSlot_2.Select(s2 => s2.VenueId).Contains(w.ven)).ToList();
                    }
            //if (iCounter > 10)
            //    break;
        }
        var List_SimilarityOrdered = List_Similarity.OrderBy(x => x.TimeSlot1).ThenBy(x => x.TimeSlot2).ToList();

        foreach (var simItem in List_SimilarityOrdered)
            Debug.WriteLine(simItem.userID.ToString() + "--> Sim(" + simItem.TimeSlot1 + "," + simItem.TimeSlot2 + ")=" + simItem.CosineSim);
        
        Debug.WriteLine("Finished Month of the year: " + DateTime.Now.ToString());        
    }
}