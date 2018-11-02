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
---------------------------------------------
Generative MATI model in recommendation.
Paper: Leveraging multi-aspect time-related influence in location recommendation.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class algorithms_TemporalInfluence : System.Web.UI.Page
{
    // Class definition.
    private class ui_lj_usgval
    {
        public int ui { get; set; }
        public string lj { get; set; }
        public double usgVal { get; set; }
    }

    private class Location_Time
    {
        public string location_id { get; set; }
        public int hour_slot { get; set; }
        public int day_slot { get; set; }
        public double usgVal { get; set; }
    }
    private class Location_Time_block
    {
        public string zt;
        public string zd;
        public int sum_visits;
        public double prob_zt_zd;
    }
    private class Prob_zt_zd_Given_ui_lj
    {
        public int ui;
        public string lj;
        public string zt;
        public string zd;
        public double Joint_prob_zt_zd_Given_ui_lj;
    }

    private class Prob_Temporal_Influences
    {
        public int ui;
        public string lj;
        public string zt;
        public string zd;
        public double dProb_zt_Given_zd_ui_lj;
        public double dProb_zd_Given_ui_lj;
        public double total_including_usg_temporal;
        public double total_only_temporal;

    }

    private class Sum_only_zd_Prob_zt_zd_Given_ui_lj
    {
        public int ui;
        public string lj;
        public string zd;
        public double Joint_prob_zt_zd_Given_ui_lj_Group_by_zd;
    }

    private class Sum_zt_Per_zd_Prob_zt_zd_Given_ui_lj
    {
        public int ui;
        public string lj;
        public string zd;
        public double Joint_prob_zt_zd_Given_ui_lj_Group_by_zd;
    }

    private class Prob_zd_Given_ui_lj
    {
        public int ui;
        public string lj;
        public string zd;
        public double dProb_zd_Givenzd_ui_lj;
    }


    public Dictionary<int, string> SimilarDays = new Dictionary<int, string>();
    public Dictionary<int, string> SimilarHours = new Dictionary<int, string>();

    protected void TemporalInfluence(object sender, EventArgs e)
    {
        Server.ScriptTimeout = 300000;
        /*
        Table: hour_smoothing
            0,1,2
            3,4
            6,7
            8,9,10
            11,12,13
            14,15
            16,17,18,19
            20,21,22,23
            5

         * Day Smoothing
         d1 5,6: Thursday and Friday
         d2 3,4: Tuesday, Wednesday
         d3 1,7: Sat., Sunday
         d4 2: Monday
         */
        //Dictionaries which report the smoothing clusters for the days and hours.
        
        SimilarDays.Add(1, "2");
        SimilarDays.Add(2, "3,4");
        SimilarDays.Add(3, "5,6");
        SimilarDays.Add(4, "7,1");

        SimilarHours.Add(1, "0,1,2");
        SimilarHours.Add(2, "3,4");
        SimilarHours.Add(3, "5");
        SimilarHours.Add(4, "6,7");
        SimilarHours.Add(5, "8,9,10");
        SimilarHours.Add(6, "11,12,13");
        SimilarHours.Add(7, "14,15");
        SimilarHours.Add(8, "16,17,18,19");
        SimilarHours.Add(9, "20,21,22,23");

        //Temporal Multiple Influence

        //Three cases: At_num,Alpha,Beta  
        /*
        Temporal_Multiple_Influence(5, 0.3, 0.4);
        Temporal_Multiple_Influence(10, 0.4, 0.4);
        Temporal_Multiple_Influence(20, 0.4, 0.4);

        Debug.WriteLine("Process Finished!");
         */

        //WE need to normalize first: 
        /*
        Added columns:
         * Avg_temporal_USG_norm
         * Jaccard_temporal_slots_norm
         * Avg_temporal_norm
         */
        /*
        Brightkite_Normalize("5");
        Brightkite_Normalize("10");
        Brightkite_Normalize("20");
         */

        //Brightkite_Ranking_Multi_Temporal_30PercentMarkOff();

        //Ok what are the users for the test?
        //Those who are considered for the case of ours.

        //Brightkite_Multi_Temporal_Iterate();
    }

    private void Temporal_Multiple_Influence(int atNumber, double USG_alpha, double USG_beta)
    {
        DataTable dt_users = Get_list_of_users();

        int iCounter = 0;
        int iTotalUsers = dt_users.Rows.Count;

        foreach (DataRow dr_user in dt_users.Rows)
        {
            iCounter++;
            Debug.Write(iCounter.ToString() + " out of " + iTotalUsers.ToString() + ". User:" + General.StringValue(dr_user[0]) + " -- " + DateTime.Now.TimeOfDay.ToString());

            GetProbTemporal_Ui_AllLj(int.Parse(General.StringValue(dr_user[0])), atNumber, USG_alpha, USG_beta);
            Debug.WriteLine("");
        }
        Debug.WriteLine("");
    }

    private void GetProbTemporal_Ui_AllLj(int CurrentUser, int atNum, double alpha, double beta)
    {
        //write a code which process all the locations of a user.

        //Start with the user
        //second we comute the the joint act for ui.

        //**************************************************************
        //The code for getting the blocks for [user ui]
        //User: CurrentUser

        //*Response.Write("<h3>Checkins of [ui]=" + CurrentUser+" in each of temporal slots.</h3>");

        DataTable dtUserCheckins = GetUsers_Checkins();

        //Debug.WriteLine("ui" + CurrentUser);        

        List<Location_Time_block> zt_zd_blocks_acts_ui = new List<Location_Time_block>();

        List<Location_Time> lstUser_History;
        lstUser_History = dtUserCheckins.AsEnumerable()
        .Select(row => new Location_Time
        {
            location_id = General.StringValue(row.Field<string>("location_id")),
            hour_slot = int.Parse(General.StringValue(row.Field<int>("hour_slot"))),
            day_slot = int.Parse(General.StringValue(row.Field<int>("day_slot"))),
            usgVal = 0.0
        }).ToList();


        for (int iday = 1; iday < 5; iday++) //Day in week 5 clusters
            for (int iHour = 1; iHour < 10; iHour++) //Hour in day clusters
            {
                List<Location_Time> block1 = lstUser_History.Where(s => SimilarHours[iHour].Split(',').Contains(s.hour_slot.ToString()) && SimilarDays[iday].Split(',').Contains(s.day_slot.ToString())).ToList<Location_Time>();
                int BlockVisits = (block1 != null) ? block1.Count() : 0;
                //*Response.Write("<br/>" + string.Format("Days:{0}, Hours:{1} -> {2} out of {3}", SimilarDays[iday], SimilarHours[iHour], BlockVisits, dtUserCheckins.Rows.Count));
                zt_zd_blocks_acts_ui.Add(new Location_Time_block { zd = SimilarDays[iday], zt = SimilarHours[iHour], sum_visits = BlockVisits, prob_zt_zd = (double)BlockVisits / (double)dtUserCheckins.Rows.Count });
            }
        //*Response.Write("<br/><br/>");

        int ui_slots_visited = zt_zd_blocks_acts_ui.Where(c => c.sum_visits > 0).Count();


        //********************************** Now we continue with the locations of the user.        
        //Instead of one location we get a list of locations belonged to the user.

        DataTable dt_ljs_currentUser = USG_Top_K_Get_checkins_of_lj();

        //Get top N location ranked for the user.
        List<Location_Time> lstLocation_Time_History_All_ljs;
        try
        {
            lstLocation_Time_History_All_ljs = dt_ljs_currentUser.AsEnumerable()
            .Select(row => new Location_Time
            {
                location_id = row.Field<string>("location_id"),
                hour_slot = row.Field<int>("hour_slot"),
                day_slot = row.Field<int>("day_slot"),
                usgVal = row.Field<double>("usgVal")
            }).ToList();
        }
        catch (Exception ex1)
        {
            Debug.WriteLine(CurrentUser + "... Failed");
            return;
        }

        Debug.Write("....");

        //Get the distinct name of locations ranked for the user.
        var uniqueLocationsOfUser = lstLocation_Time_History_All_ljs.Select(l => l.location_id)
                                  .Distinct();

        List<ui_lj_usgval> lst_ui_lj_usgval = lstLocation_Time_History_All_ljs
                .GroupBy
                (ac => new
                {
                    ac.location_id,
                    ac.usgVal
                })
                .Select(cl => new ui_lj_usgval
                {
                    ui = CurrentUser,
                    lj = cl.First().location_id,
                    usgVal = cl.First().usgVal
                }).ToList();


        foreach (var ui_lj_item in lst_ui_lj_usgval)
        {
            //*Response.Write("<br/><br/><b>Current Location:</b>" + ui_lj_item.lj.ToString());

            List<Location_Time> lstLocation_Time_History = lstLocation_Time_History_All_ljs.Where(x => x.location_id == ui_lj_item.lj).ToList<Location_Time>();

            //*Response.Write(string.Format("<br/><b>{0} checkins</b> on this location", lstLocation_Time_History.Count()));

            /*
            List<Location_Time> lstLocation_Time_History;
            lstLocation_Time_History = dt_lj.AsEnumerable()
            .Select(row => new Location_Time
            {
                location_id = row.Field<string>("location_id"),
                hour_slot = row.Field<int>("hour_slot"),
                day_slot = row.Field<int>("day_slot")
            }).ToList();
                */
            //*Response.Write("<br/>lstLocation_Time_History.Count: " + lstLocation_Time_History.Count.ToString());

            List<Location_Time_block> zt_zd_blocks_acts_lj = new List<Location_Time_block>();

            for (int iday = 1; iday < 5; iday++) //Day in week 5 clusters
                for (int iHour = 1; iHour < 10; iHour++) //Hour in day clusters
                {
                    List<Location_Time> block1 = lstLocation_Time_History.Where(s => SimilarHours[iHour].Split(',').Contains(s.hour_slot.ToString()) && SimilarDays[iday].Split(',').Contains(s.day_slot.ToString())).ToList<Location_Time>();
                    int BlockVisits = (block1 != null) ? block1.Count() : 0;
                    //*Response.Write("<br/>" + string.Format("Days:{0}, Hours:{1} -> <b>{2}</b> out of <b>{3}</b>", SimilarDays[iday], SimilarHours[iHour], BlockVisits, lstLocation_Time_History.Count()));
                    //*Response.Write("<br/>" + string.Format("{0}", BlockVisits));

                    zt_zd_blocks_acts_lj.Add(new Location_Time_block { zd = SimilarDays[iday], zt = SimilarHours[iHour], sum_visits = BlockVisits, prob_zt_zd = (double)BlockVisits / (double)lstLocation_Time_History.Count() });
                }
            //*Response.Write("<br/><br/>");

            int lj_slots_visited = zt_zd_blocks_acts_lj.Where(c => c.sum_visits > 0).Count();

            //Now we have two acts: zt_zd_blocks_acts_lj and zt_zd_blocks_acts_ui
            //*Response.Write("<h3>Now we have two acts: zt_zd_blocks_acts_lj and zt_zd_blocks_acts_ui</h3>");

            //First location act during zt, zd time blocks(slots)
            //*Response.Write("<h3>zt_zd_blocks_acts_lj</h3>");
            //*foreach (var act in zt_zd_blocks_acts_lj)
            //*Response.Write(string.Format("<br/>act.zt:{0},act.zd:{1},act.sum_visits:{2} out of {3},act.prob_zt_zd:{4}", act.zt, act.zd, act.sum_visits, lstLocation_Time_History.Count(), act.prob_zt_zd));

            //*Response.Write("<h3>zt_zd_blocks_acts_ui</h3>");
            //*foreach (var act in zt_zd_blocks_acts_ui)
            //*Response.Write(string.Format("<br/>act.zt:{0},act.zd:{1},act.sum_visits:{2} out of {3},act.prob_zt_zd:{4}", act.zt, act.zd, act.sum_visits, dtUserCheckins.Rows.Count, act.prob_zt_zd));

            //*Response.Write("<br/><br/>");

            //We want to compute the User_Location_Temporal_Acts pr(zt,zd|ui,lj)
            List<Prob_zt_zd_Given_ui_lj> Prob_zt_zd_Given_ui_lj_List = new List<Prob_zt_zd_Given_ui_lj>();

            //Loop through Hour/Day blocks.
            //*for (int iday = 1; iday < 5; iday++) //Day in week 5 clusters
            //*for (int iHour = 1; iHour < 10; iHour++) //Hour in day clusters
            //*Response.Write("<br/>" + string.Format("Days:{0}, Hours:{1}", SimilarDays[iday], SimilarHours[iHour]));

            //Convert it to enumerator. then we can iterate through them one by one.
            IEnumerator<Location_Time_block> zt_zd_blocks_acts_lj_Enum = zt_zd_blocks_acts_lj.GetEnumerator();
            IEnumerator<Location_Time_block> zt_zd_blocks_acts_ui_Enum = zt_zd_blocks_acts_ui.GetEnumerator();

            double Total_Prob_lj_ui = 0.0;
            //Iterate through li and ui.
            while ((zt_zd_blocks_acts_lj_Enum.MoveNext()) && (zt_zd_blocks_acts_ui_Enum.MoveNext()))
            {

                //* 
                /*Response.Write(string.Format("<br/>lj({0}&{1}):{2}-ui({3}&{4}):{5}",
                    zt_zd_blocks_acts_lj_Enum.Current.zt,
                    zt_zd_blocks_acts_lj_Enum.Current.zd,
                    zt_zd_blocks_acts_lj_Enum.Current.prob_zt_zd,

                    zt_zd_blocks_acts_ui_Enum.Current.zt,
                    zt_zd_blocks_acts_ui_Enum.Current.zd,
                    zt_zd_blocks_acts_ui_Enum.Current.prob_zt_zd

                    ));*/

                double Sum_Of_Prob_lj_ui = 0.0;

                //If both ui and lj have activity in the time block then we consider it.
                if (zt_zd_blocks_acts_ui_Enum.Current.prob_zt_zd > 0 && zt_zd_blocks_acts_lj_Enum.Current.prob_zt_zd > 0)
                {
                    //We sum them first as we don't know the total. Then for the whole list we devide it with the sum
                    Sum_Of_Prob_lj_ui = zt_zd_blocks_acts_ui_Enum.Current.prob_zt_zd + zt_zd_blocks_acts_lj_Enum.Current.prob_zt_zd;
                    Total_Prob_lj_ui += Sum_Of_Prob_lj_ui;
                }
                Prob_zt_zd_Given_ui_lj_List.Add(new Prob_zt_zd_Given_ui_lj
                {
                    ui = CurrentUser,
                    lj = ui_lj_item.lj.ToString(),
                    zt = zt_zd_blocks_acts_ui_Enum.Current.zt,
                    zd = zt_zd_blocks_acts_ui_Enum.Current.zd,
                    Joint_prob_zt_zd_Given_ui_lj = Sum_Of_Prob_lj_ui
                });
            }

            //Now the value in Joint_prob_zt_zd_Given_ui_lj is just sum, we need to divide it with the total sum of all te             
            Prob_zt_zd_Given_ui_lj_List.ForEach(i => i.Joint_prob_zt_zd_Given_ui_lj = (Total_Prob_lj_ui > 0.0) ? (i.Joint_prob_zt_zd_Given_ui_lj / Total_Prob_lj_ui) : 0.0);

            //*Response.Write("<h2>pr(zt,zd|ui,lj) using the act</h2>");
            Prob_zt_zd_Given_ui_lj_List.OrderByDescending(x => x.Joint_prob_zt_zd_Given_ui_lj);

            //Response.Clear();

            //Sort the list desc, based on the act based joint probability of pr(zt,zd|ui,lj).
            //foreach (var i in Prob_zt_zd_Given_ui_lj_List.OrderByDescending(x => x.Joint_prob_zt_zd_Given_ui_lj))
            //*foreach (var i in Prob_zt_zd_Given_ui_lj_List.OrderBy(x => x.zt).ThenBy(x=>x.zd))
            //*Response.Write("<BR/>"+i.Joint_prob_zt_zd_Given_ui_lj);
            //*Response.Write(string.Format("<BR/>pr_Act(zt,zd|ui,lj)=>pr_Act({0} and {1}|{2} and {3})={4}", i.zt, i.zd, "i.ui", "i.lj", i.Joint_prob_zt_zd_Given_ui_lj));

            //Here is the rest. Code for pr(zt|zd,ui,lj)=sum_Zd(pr(zt|zd,ui,lj))/sum_Zd sum_Zt(pr(zt,zd|ui,lj))        
            //In fact we get the sum of the probability when they are zt. then divide it with the sum of all.

            //************************************
            //List of p(zt|zd,ui,lj)        
            //Prob(zt|zd,ui,lj)        
            //(sum of the current zt for the day zd)/(Sum of all zts for the day zd)

            //*Response.Write("<br/><br/>************************************Prob(zt|zd,ui,lj)=(sum of the current zt for the day zd)/(Sum of all zts for the day zd)************************************<br/>");

            //First create a list which keeps the sum of acts for a day slot (zd) which is the sum of all joint probability of zt and zd for the day block zd.
            List<Sum_only_zd_Prob_zt_zd_Given_ui_lj> lst_Sum_Prob_zt_zd_Given_ui_lj_groupby_zd = Prob_zt_zd_Given_ui_lj_List
                    .GroupBy(l => l.zd) //Group by zt to sum up the act join prob for the day.
                    .Select(cl => new Sum_only_zd_Prob_zt_zd_Given_ui_lj
                    {
                        ui = CurrentUser,
                        lj = ui_lj_item.lj.ToString(),
                        zd = cl.First().zd,
                        Joint_prob_zt_zd_Given_ui_lj_Group_by_zd = cl.Sum(c => c.Joint_prob_zt_zd_Given_ui_lj)
                    }).ToList();

            //*foreach (var i in lst_Sum_Prob_zt_zd_Given_ui_lj_groupby_zd)
            //*Response.Write(string.Format("<BR/>Sum pr_Act(zt,zd|ui,lj)=>Day: {0}={1}", i.zd, i.Joint_prob_zt_zd_Given_ui_lj_Group_by_zd));

            //Now we devide each joint probability of zt,zd (Prob_zt_zd_Given_ui_lj_List) by the sum of relavant day (lst_Sum_Prob_zt_zd_Given_ui_lj_groupby_zd) as formulated in (27) in the paper.
            var var_Temporal_Influences = from jointProb in Prob_zt_zd_Given_ui_lj_List
                                          join sumgroup in lst_Sum_Prob_zt_zd_Given_ui_lj_groupby_zd on jointProb.zd equals sumgroup.zd into gj
                                          from FinalJoin in gj.DefaultIfEmpty()
                                          select new Prob_Temporal_Influences { ui = CurrentUser, lj = ui_lj_item.lj.ToString(), zt = jointProb.zt, zd = jointProb.zd, dProb_zd_Given_ui_lj = FinalJoin.Joint_prob_zt_zd_Given_ui_lj_Group_by_zd, dProb_zt_Given_zd_ui_lj = (FinalJoin.Joint_prob_zt_zd_Given_ui_lj_Group_by_zd == 0 ? 0 : jointProb.Joint_prob_zt_zd_Given_ui_lj / FinalJoin.Joint_prob_zt_zd_Given_ui_lj_Group_by_zd) };

            List<Prob_Temporal_Influences> lst_Temporal_Influences = var_Temporal_Influences.ToList();

            lst_Temporal_Influences.ForEach(i => i.total_including_usg_temporal = Math.Pow(10.0, (Math.Log10(i.dProb_zd_Given_ui_lj) + Math.Log10(i.dProb_zt_Given_zd_ui_lj) + Math.Log10(ui_lj_item.usgVal))));
            lst_Temporal_Influences.ForEach(i => i.total_only_temporal = Math.Pow(10.0, (Math.Log10(i.dProb_zd_Given_ui_lj) + Math.Log10(i.dProb_zt_Given_zd_ui_lj))));

            //Temporal Influence:
            //1. dProb_zd_Given_ui_lj
            //2. dProb_zt_Given_zd_ui_lj

            //Usg Influence:
            //3. ui_lj_item.usgVal

            //*Response.Write("<br/><br/><br/>Report on total including usg temporal");

            //foreach (var v in lst_Temporal_Influences)
            //Debug.WriteLine("<br/>"+ui_lj_item.usgVal+"["+v.zt+"]   ["+v.zd+"]  --> Full:"+v.total_including_usg_temporal+" -- T:"+ v.total_only_temporal);

            ////*Response.Write(string.Format("<BR/>Pr.(ui,lj,zt=[{0}],zd=[{1}])={2} ==> {3} ", v.zt, v.zd, (v.dProb_zd_Given_ui_lj*v.dProb_zt_Given_zd_ui_lj*ui_lj_item.usgVal), v.total_including_usg_temporal));
            ////*Response.Write(string.Format("<BR/>dProb_zt_Given_zd_ui_lj(zt={0}|zd={1},ui,lj)={2} *** dProb_zt_Given_zd_ui_lj(zd={1}|ui,lj)={3} ==> {4},{5} ", v.zt, v.zd, v.dProb_zt_Given_zd_ui_lj, v.dProb_zd_Given_ui_lj, v.log_total_temporal_influence, ui_lj_item.usgVal));


            //double avg_total_including_usg_temporal = double.Parse(lst_Temporal_Influences.Where(pr => pr.total_including_usg_temporal > 0).Average(x => x.total_including_usg_temporal).ToString());
            //double avg_total_only_temporal = double.Parse(lst_Temporal_Influences.Where(pr => pr.total_only_temporal > 0).Average(x => x.total_only_temporal).ToString());

            double avg_total_including_usg_temporal = 0.0;
            if (lst_Temporal_Influences.Where(pr => pr.total_including_usg_temporal > 0).Count() > 0)
                avg_total_including_usg_temporal = double.Parse(lst_Temporal_Influences.Where(pr => pr.total_including_usg_temporal > 0).Average(x => x.total_including_usg_temporal).ToString());

            double avg_total_only_temporal = 0.0;
            if (lst_Temporal_Influences.Where(pr => pr.total_only_temporal > 0).Count() > 0)
                avg_total_only_temporal = double.Parse(lst_Temporal_Influences.Where(pr => pr.total_only_temporal > 0).Average(x => x.total_only_temporal).ToString());

            //Number of shared slots for ui and lj
            int lj_ui_shared_slots_num = lst_Temporal_Influences.Where(pr => pr.total_including_usg_temporal > 0).Count();

            //Debug.WriteLine("");
            //Debug.WriteLine("");
            //Debug.WriteLine("");

            //Debug.WriteLine("<br/><br/>\\psi(ui,lj)=" + lj_ui_shared_slots_num.ToString() + "/" + ui_slots_visited + "+" + lj_slots_visited);
            double psi_i_j = (double)lj_ui_shared_slots_num / (double)(ui_slots_visited + lj_slots_visited);
            //Debug.WriteLine("Average of none zero temporal probabilities pr(ui,lj,zt,zd): <b>Include USG:</b> " + avg_total_including_usg_temporal + " -- <b>Exclude USG:</b> " + avg_total_only_temporal);

            //ui_lj_item.ui
            //ui_lj_item.lj

            string[] pnames = new string[] { "usid", "location_id", "atNum", "alpha", "beta", "usgval", "Avg_temporal_USG", "Avg_temporal", "Jaccard_temporal_slots" };
            object[] pvals = new object[] { 
                                    General.StringValue(ui_lj_item.ui),
                                    General.StringValue(ui_lj_item.lj),                                    
                                    General.StringValue(atNum),
                                    General.StringValue(alpha),
                                    General.StringValue(beta),
                                    General.StringValue(ui_lj_item.usgVal),
                                    General.StringValue(avg_total_including_usg_temporal),
                                    General.StringValue(avg_total_only_temporal),
                                    General.StringValue(psi_i_j)
                };


            //*Response.Flush();
        }
        Debug.Write("Success");
    }
}