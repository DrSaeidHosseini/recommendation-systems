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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;

public partial class algorithms_Expectation_Maximization_Prob : System.Web.UI.Page
{

    private class E_Step
    {
        private double m_f1;
        private double m_f2;
        private double m_s;
        private double m_Prob_s_Gvn_f1_f2;

        public E_Step()
        {
        }

        public E_Step(double f1, double f2, double s, double probs_Givenf1f2)
        {
            m_f1 = f1;
            m_f2 = f2;
            m_s = s;
            m_Prob_s_Gvn_f1_f2 = probs_Givenf1f2;
        }

        public double f_1
        {
            get { return m_f1; }
            set { m_f1 = value; }
        }

        public double f_2
        {
            get { return m_f2; }
            set { m_f2 = value; }
        }

        public double s
        {
            get { return m_s; }
            set { m_s = value; }
        }

        public double Prob_s_Gvn_f1_f2
        {
            get { return m_Prob_s_Gvn_f1_f2; }
            set { m_Prob_s_Gvn_f1_f2 = value; }
        }
    }

    private class Prob_s_obj
    {
        private double m_sID;
        private double m_Prob_s;       

        public Prob_s_obj()
        {
        }

        public Prob_s_obj(double i_sID, double i_Prob_s)
        {
            m_sID = i_sID;
            m_Prob_s = i_Prob_s;
        }

        public double sID
        {
            get { return m_sID; }
            set { m_sID = value; }
        }

        public double Prob_s
        {
            get { return m_Prob_s; }
            set { m_Prob_s = value; }
        }
    }

    private class Prob_f1_Given_s_obj
    {
        private double m_sID;
        private double m_sf1;
        private double m_Prob_f1_Given_s;

        public Prob_f1_Given_s_obj()
        {
        }

        public Prob_f1_Given_s_obj(double i_sID, double i_sf1, double i_Prob_f1_Given_s)
        {
            m_sID = i_sID;
            m_sf1 = i_sf1;
            m_Prob_f1_Given_s = i_Prob_f1_Given_s;
        }

        public double sID
        {
            get { return m_sID; }
            set { m_sID = value; }
        }

        public double sf1
        {
            get { return m_sf1; }
            set { m_sf1 = value; }
        }

        public double Prob_f1_Given_s
        {
            get { return m_Prob_f1_Given_s; }
            set { m_Prob_f1_Given_s = value; }
        }
    }


    private class Prob_f2_Given_s_obj
    {
        private double m_sID;
        private double m_sf2;
        private double m_Prob_f2_Given_s;

        public Prob_f2_Given_s_obj()
        {
        }

        public Prob_f2_Given_s_obj(double i_sID, double i_sf2, double i_Prob_f2_Given_s)
        {
            m_sID = i_sID;
            m_sf2 = i_sf2;
            m_Prob_f2_Given_s = i_Prob_f2_Given_s;
        }

        public double sID
        {
            get { return m_sID; }
            set { m_sID = value; }
        }

        public double sf2
        {
            get { return m_sf2; }
            set { m_sf2 = value; }
        }

        public double Prob_f2_Given_s
        {
            get { return m_Prob_f2_Given_s; }
            set { m_Prob_f2_Given_s = value; }
        }
    }

    private class s_f1_obj
    {
        private double m_f1;
        private double m_s;
        private int m_Count;

        public s_f1_obj()
        {
        }

        public s_f1_obj(double f1, double s, int Num_Count)
        {
            m_f1 = f1;            
            m_s = s;
            m_Count = Num_Count;
        }

        public double sf1
        {
            get { return m_f1; }
            set { m_f1 = value; }
        }

        public double sID
        {
            get { return m_s; }
            set { m_s = value; }
        }

        public int sCount
        {
            get { return m_Count; }
            set { m_Count = value; }
        }
    }

    private class s_f2_obj
    {
        private double m_f2;
        private double m_s;
        private int m_Count;

        public s_f2_obj()
        {
        }

        public s_f2_obj(double f2, double s, int Num_Count)
        {
            m_f2 = f2;
            m_s = s;
            m_Count = Num_Count;
        }

        public double sf2
        {
            get { return m_f2; }
            set { m_f2 = value; }
        }

        public double sID
        {
            get { return m_s; }
            set { m_s = value; }
        }

        public int sCount
        {
            get { return m_Count; }
            set { m_Count = value; }
        }
    }

    double EM_Convergence_Epsilon = 0.1;
    protected int Theta_Items_counter = 0;
    protected double[] Theta_Old = new double[15];
    protected double[] Theta_New = new double[15];

    List<Prob_s_obj> Prob_s;
    List<Prob_f1_Given_s_obj> Prob_f1_Given_s;
    List<Prob_f2_Given_s_obj> Prob_f2_Given_s;
    List<E_Step> Feature_sense;

    //# of features is 2
    int Num_Features = 2;
    int Num_Senses = 3;
    int N;

    protected void EM_method(object sender, EventArgs e)
    {

        //Clear both the arrays - set all the items to zero.
        Array.Clear(Theta_Old, 0, Theta_Old.Length);
        Array.Clear(Theta_New, 0, Theta_New.Length);

        //Load the Data first into dt1 data table.        

        //Get the total number of observation.
        N = int.Parse(dt1.Rows.Count.ToString());

        //Features are F1 and F2 and S is the sense of the bill.
        //As required for the first iteration we have already initialized the Sense of the bill with some random values.
        DataView view = new DataView(dt1);
        
        view.Sort = "f1"; //Distinct vals for feature 1
        DataTable distinct_f1 = view.ToTable(true, "f1");

        view.Sort = "f2"; //Distinct vals for feature 2
        DataTable distinct_f2 = view.ToTable(true, "f2");

        //Distinct vals for S: Sense of bill
        // S has been initiated by random numbers.
        view.Sort = "s"; 
        DataTable distinct_s = view.ToTable(true, "s");
                
        GridObservation.DataSource = dt1;
        GridObservation.DataBind();

        //We have initialized the value for Sense of the Bill: S, S is incomplete.
        //we have two features: F1 and F2, S is sense of the bill: S

        Response.Write("<br/>********** E-Step 1 ****************");
        Response.Write("<br/>E-Step 1 doesn't have any process. We have assigned them random values.");

        //Create the initial list which will be used during the whole process. in all iterations. The value for the sense will be updated though.        
        Feature_sense = dt1.AsEnumerable()
        .Select(row => new E_Step
        {
            f_1 = row.Field<double>("f1"),
            f_2 = row.Field<double>("f2"),
            s = row.Field<double>("s")
        }).ToList();

        //E-Step is about calculation of arg_Max P(S|F1,F2)
        //M-Step is about Re-assigning of S Value which will influence new values for P(s), P(F1|s) and P(F1|s)
                        
        /********** E-Step 1 ****************/
        //Get the group by S and the probability.
        var Groupby_S = from row1 in dt1.AsEnumerable()
                        group row1 by row1.Field<double>("s") into grp
                        orderby grp.Key
                        select new
                        {
                            sID = grp.Key,
                            sCount = grp.Count()
                        };

        //Freq(s)
        //foreach (var t in Groupby_S)
        //    Response.Write(string.Format("<br/>s:{0}, Freq:{1}",t.sID,t.sCount));

        //Prob(s)
        var Prob_s1 = from s in Groupby_S
                      select new Prob_s_obj() { sID = s.sID, Prob_s = Math.Round(((float)s.sCount / N) * 1000) / 1000 };

        Prob_s = Prob_s1.ToList();

        //Response.Write("<br/>*********** P(s) *************");

        //Reporting Prob(s)
        foreach (var t in Prob_s)
        {
            Response.Write(string.Format("<br/>s:{0}, P(s):{1}", t.sID, t.Prob_s));
            Theta_New[Theta_Items_counter++] = t.Prob_s;
        }

        Response.Write("<br/>********** M-Step 1 ****************");
        
        //S [Randomly Initialized], So: MLE works. Complete Data case
        //Freq(f1,S)
        var Groupby_f1_S_init = from row1 in dt1.AsEnumerable()
                           group row1 by new  { s = row1.Field<double>("s"), f1 = row1.Field<double>("f1") } into grp
                           orderby grp.Key.f1, grp.Key.s
                           select new s_f1_obj()
                           {
                               sf1 = grp.Key.f1,                               
                               sID = grp.Key.s,
                               sCount = grp.Count()
                           };

        List<s_f1_obj> Groupby_f1_S = Groupby_f1_S_init.ToList();

        for (int i = 1; i <= Num_Features; i++)
                for (int k = 1; k <= Num_Senses; k++)
                    if (!Groupby_f1_S.Any(x => x.sf1 == i && x.sID == k))
                        Groupby_f1_S.Add(new s_f1_obj(i, k,0));

        foreach (var t in Groupby_f1_S)
        {
            //Response.Write(string.Format("<br/>sID:{0}, sf1:{1}, Count:{2}", t.sID, t.sf1, t.sCount));
        }

        //Prob(f1|s)
        var Prob_f1_Given_s1 = from s in Groupby_f1_S
                               select new Prob_f1_Given_s_obj { sID = s.sID, sf1 = s.sf1, Prob_f1_Given_s = (float)int.Parse(s.sCount.ToString()) / Groupby_S.Where(x => x.sID == s.sID).FirstOrDefault().sCount };

        Prob_f1_Given_s = Prob_f1_Given_s1.ToList();
        
        foreach (var t in Prob_f1_Given_s.OrderBy(x => x.sID).ThenBy(x => x.sf1))
        {
            Response.Write(string.Format("<br/>Prob(f1={1}|s={0})={2}", t.sID, t.sf1, t.Prob_f1_Given_s));
            Theta_New[Theta_Items_counter++] = t.Prob_f1_Given_s;
        }
        
        Response.Write("<br/>");

        var Groupby_f2_S_init = from row1 in dt1.AsEnumerable()
                           group row1 by new { s = row1.Field<double>("s"), f2 = row1.Field<double>("f2") } into grp
                           orderby grp.Key.s, grp.Key.f2
                                select new s_f2_obj
                           {
                               sf2 = grp.Key.f2,
                               sID = grp.Key.s,
                               sCount = grp.Count()
                           };

        List<s_f2_obj> Groupby_f2_S = Groupby_f2_S_init.ToList();

        for (int i = 1; i <= Num_Features; i++)
            for (int k = 1; k <= Num_Senses; k++)
                if (!Groupby_f2_S.Any(x => x.sf2 == i && x.sID == k))
                    Groupby_f2_S.Add(new s_f2_obj(i, k, 0));

        //foreach (var t in Groupby_f2_S)
        //    Response.Write(string.Format("<br/>sID:{0}, sf2:{1}, Count:{2}", t.sID, t.sf2, t.sCount));

        //Prob(f2|s)
        var Prob_f2_Given_s1 = from s in Groupby_f2_S
                              select new Prob_f2_Given_s_obj { sID = s.sID, sf2 = s.sf2, Prob_f2_Given_s = (float)int.Parse(s.sCount.ToString()) / Groupby_S.Where(x => x.sID == s.sID).First().sCount };

        Prob_f2_Given_s = Prob_f2_Given_s1.ToList();

        foreach (var t in Prob_f2_Given_s.OrderBy(x => x.sID).ThenBy(x => x.sf2))
        {   
            //if(t.sf2==2)
            Response.Write(string.Format("<br/>Prob(f2={1}|s={0})={2}", t.sID, t.sf2, t.Prob_f2_Given_s));
            Theta_New[Theta_Items_counter++] = t.Prob_f2_Given_s;
        }

        //Input: P(s) P(f1|s) P(f2|s)
        //Output: P(s) P(f1|s) P(f2|s)
        //Convergence Factor: Value for the convergence.

        for(int i = 2; i < 2000; i++)
        {
            double Euclidean_Model_Parameter_Distance_From_Previous_Iteration = E_M_Iteration(i);
            Response.Write("<br/>Euclidean Model Parameter Distance From Previous Iteration: " + Euclidean_Model_Parameter_Distance_From_Previous_Iteration.ToString());
            if (Euclidean_Model_Parameter_Distance_From_Previous_Iteration < 0.1)
            {
                Response.Write("<br/>&bull;<h2 style='color:green;'>Last Iteration:" + (i.ToString()) + "</h2>");
                Response.Write("<br><span style='color:red'>Reporting Final model parameters:<b>P(s) p(f1|s) p(f2|s)</b></span>");

                //Prob_s                
                foreach (var t in Prob_s)
                    Response.Write(string.Format("<br/>s:{0}, P(s):{1}", t.sID, t.Prob_s));
                
                //Prob_f1_Given_s
                foreach (var t in Prob_f1_Given_s.OrderBy(x => x.sID).ThenBy(x => x.sf1))
                    Response.Write(string.Format("<br/>Prob(f1={1}|s={0})={2}", t.sID, t.sf1, t.Prob_f1_Given_s));

                //Prob_f2_Given_s
                foreach (var t in Prob_f2_Given_s.OrderBy(x => x.sID).ThenBy(x => x.sf2))
                    Response.Write(string.Format("<br/>Prob(f2={1}|s={0})={2}", t.sID, t.sf2, t.Prob_f2_Given_s));

                break;
            }
            else
                Response.Write("<br/><h3>E_M_Iteration step " + (i.ToString()) + " Completed.</h3>");
        }

        Response.End();

        //E-Step and m-step should repeat until converge.
        //Report the theta: What is it? When to report?
        //The time to report is when we get the P(s|f1,f2)        
        //Write a converge function:
    }
    protected double E_M_Iteration(int step)
    {
        //# of Senses is 3
        Response.Write("<br/>********** E-Step " + step.ToString() + " ****************");
        //Calculate Prob(S|F1,F2)        
        var Prob_Joint_s_f1_f2 = from s in Prob_s
                                 join f1_Given_s in Prob_f1_Given_s on s.sID equals f1_Given_s.sID
                                 join f2_Given_s in Prob_f2_Given_s on f1_Given_s.sID equals f2_Given_s.sID
                                 orderby f1_Given_s.sf1, f2_Given_s.sf2, s.sID
                                 select new
                                 {
                                     sID = s.sID,
                                     sf1 = f1_Given_s.sf1,
                                     sf2 = f2_Given_s.sf2,
                                     Prob_s = s.Prob_s,
                                     P_f1_Gvn_s = f1_Given_s.Prob_f1_Given_s,
                                     P_f2_Gvn_s = f2_Given_s.Prob_f2_Given_s,
                                     Ps_X_Pf1s_X_Pf2s = s.Prob_s * f1_Given_s.Prob_f1_Given_s * f2_Given_s.Prob_f2_Given_s
                                 };

        //foreach (var p in Prob_Joint_s_f1_f2)
        //    Response.Write(string.Format("<br/>f1:<b>{1}</b>-f2:<b>{2}</b>-S:<b>{0}</b>-->Prob(s={0}):{3},Prob(f1={1}|s={0}):{4}, Prob(f2={2}|s={0}):{5}==>{6}", p.sID, p.sf1, p.sf2, p.Prob_s, p.P_f1_Gvn_s, p.P_f2_Gvn_s, p.Ps_X_Pf1s_X_Pf2s));

        //Now let's sum based on the f1 and f2
        var Prob_Joint_s_f1_f2_grpd = Prob_Joint_s_f1_f2
            .GroupBy(ac => new
            {
                ac.sf1,
                ac.sf2
            })
     .Select(s => new
     {
         sf1 = s.Key.sf1,
         sf2 = s.Key.sf2,
         Sum = s.Sum(x => x.Ps_X_Pf1s_X_Pf2s)
     });

        //foreach (var sum in Prob_Joint_s_f1_f2_grpd)
        //    Response.Write("<br/>"+sum.sf1+ " **  "+sum.sf2+" --->"+sum.Sum);

        //Now we can have the final values for e-Step 2
        var e_step2 = from p in Prob_Joint_s_f1_f2
                      join p_sum in Prob_Joint_s_f1_f2_grpd on new { p.sf1, p.sf2 } equals new { p_sum.sf1, p_sum.sf2 }
                      orderby p.sf1, p.sf2, p.sID
                      select new
                      {
                          sID = p.sID,
                          sf1 = p.sf1,
                          sf2 = p.sf2,
                          Prob_s = p.Prob_s,
                          P_f1_Gvn_s = p.P_f1_Gvn_s,
                          P_f2_Gvn_s = p.P_f2_Gvn_s,
                          Ps_X_Pf1s_X_Pf2s = p.Ps_X_Pf1s_X_Pf2s,
                          P_s_gvn_f1_f2 = Math.Round((p.Ps_X_Pf1s_X_Pf2s / p_sum.Sum) * 1000) / 1000
                      };

        foreach (var p in e_step2)
            Response.Write(string.Format("<br/>f1:<b>{1}</b>-f2:<b>{2}</b>-S:<b>{0}</b>-->Prob(s={0}):{3},Prob(f1={1}|s={0}):{4}, Prob(f2={2}|s={0}):{5}==>P_s_gvn_f1_f2:{6}", p.sID, p.sf1, p.sf2, p.Prob_s, p.P_f1_Gvn_s, p.P_f2_Gvn_s, p.P_s_gvn_f1_f2));

        Response.Write("<br/>Now find the arg_Max P(S|F1,F2)");
        //Foe each f1 and f1 based on what s the value P(S|F1,F2) will be bigger.

        var Max_ps_gvn_f1f2 = from e_step in e_step2
                              group e_step by new { e_step.sf1, e_step.sf2 } into P_s_GivenF1F2_Grp_f1f2
                              let top_P = P_s_GivenF1F2_Grp_f1f2.OrderByDescending(x => x.P_s_gvn_f1_f2).First().P_s_gvn_f1_f2
                              let max_s1 = P_s_GivenF1F2_Grp_f1f2.OrderByDescending(x => x.P_s_gvn_f1_f2).First().sID
                              select new
                              {
                                  sf1 = P_s_GivenF1F2_Grp_f1f2.Key.sf1,
                                  sf2 = P_s_GivenF1F2_Grp_f1f2.Key.sf2,
                                  Max_ps_gvn_f1f1_Grp = top_P,
                                  sID = max_s1
                              };

        foreach (var p in Max_ps_gvn_f1f2)
            Response.Write("<br/>(" + p.sf1 + "," + p.sf2 + " Max Sense:" + p.sID + ") -> Max " + p.Max_ps_gvn_f1f1_Grp);

        Response.Write("<br/>Now we know For each f1 and f1 based on what s the value P(S|F1,F2) will be maximized [computed in ** E-Step "+step.ToString()+" ** ].");

        Response.Write("<br/>so we assign the s for each f1,f2 based on the one which maximizes the probability.<br/>therefore we will have the new table of fatures and sense of the bill.<br/> Based on which we can calculate new values for P(s), P(F1|s) and P(F1|s)");

        //Update the feature sense.
        var pairs = from d in Feature_sense
                    join b in Max_ps_gvn_f1f2
                    on new { sf_1 = d.f_1, sf_2 = d.f_2 } equals new { sf_1 = b.sf1, sf_2 = b.sf2 }
                    select new { b, d };

        foreach (var pair in pairs)
            pair.d.s = pair.b.sID;

        Response.Write("<br/>pair.d includes the new values for f1,f2 and S.");

        foreach (var pair in pairs)
            Response.Write("<br/>" + pair.d.f_1 + "," + pair.d.f_2 + "|" + pair.d.s);

        Response.Write("<br/>********** M-Step " + step.ToString() + " ****************");
        Response.Write("<br/>Re-assigning of P(s), P(F1|s) and P(F2|s) based on the new values found in E-Step "+step.ToString());

        var Groupby_S = from row1 in pairs
                         group row1 by row1.d.s into grp
                         orderby grp.Key
                         select new
                         {
                             sID = grp.Key,
                             sCount = grp.Count()
                         };

        //Freq(s)
        //foreach (var t in Groupby_S2)
        //    Response.Write(string.Format("<br/>s:{0}, Freq:{1}", t.sID, t.sCount));

        //Prob(s)
        //var Prob_s2 = from s in Groupby_S2
        //              select new { sID = s.sID, Prob_s = Math.Round(((float)s.sCount / N) * 1000) / 1000 };

        var Prob_s1 = from s in Groupby_S
                      select new Prob_s_obj() { sID = s.sID, Prob_s = Math.Round(((float)s.sCount / N) * 1000) / 1000 };

        Prob_s = Prob_s1.ToList();

        //Make a backup from last values of the thetha.
        Array.Copy(Theta_New, Theta_Old, 15);

        //we set it to zero again. to fill in the new values again.
        //Before that we need to back up the current one.
        Theta_Items_counter = 0;

        //Reporting Prob(s)
        foreach (var t in Prob_s)
        {
            Response.Write(string.Format("<br/>s:{0}, P(s):{1}", t.sID, t.Prob_s));
            Theta_New[Theta_Items_counter++] = t.Prob_s;
        }

        //S
        //Freq(f1,S)
        var Groupby_f1_S_init2 = from row1 in pairs
                                 group row1 by new { s = row1.d.s, f1 = row1.d.f_1 } into grp
                                 orderby grp.Key.f1, grp.Key.s
                                 select new s_f1_obj()
                                 {
                                     sf1 = grp.Key.f1,
                                     sID = grp.Key.s,
                                     sCount = grp.Count()
                                 };

        List<s_f1_obj> Groupby_f1_S2 = Groupby_f1_S_init2.ToList();

        for (int i = 1; i <= Num_Features; i++)
            for (int k = 1; k <= Num_Senses; k++)
                if (!Groupby_f1_S2.Any(x => x.sf1 == i && x.sID == k))
                    Groupby_f1_S2.Add(new s_f1_obj(i, k, 0));

        //foreach (var t in Groupby_f1_S2)
        //    Response.Write(string.Format("<br/>sID:{0}, sf1:{1}, Count:{2}", t.sID, t.sf1, t.sCount));

        var Prob_f1_Given_s1 = from s in Groupby_f1_S2
                               select new Prob_f1_Given_s_obj { sID = s.sID, sf1 = s.sf1, Prob_f1_Given_s = (float)int.Parse(s.sCount.ToString()) / Groupby_S.Where(x => x.sID == s.sID).FirstOrDefault().sCount };

        Prob_f1_Given_s = Prob_f1_Given_s1.ToList();

        Response.Write("<BR/>P(F1|s)");
        foreach (var t in Prob_f1_Given_s.OrderBy(x => x.sID).ThenBy(x => x.sf1))
        {
            Response.Write(string.Format("<br/>Prob(f1={1}|s={0})={2}", t.sID, t.sf1, t.Prob_f1_Given_s));
            Theta_New[Theta_Items_counter++] = t.Prob_f1_Given_s;
        }

        Response.Write("<br/>");

        var Groupby_f2_S_init2 = from row1 in pairs
                                 group row1 by new { s = row1.d.s, f2 = row1.d.f_2 } into grp
                                 orderby grp.Key.s, grp.Key.f2
                                 select new s_f2_obj
                                 {
                                     sf2 = grp.Key.f2,
                                     sID = grp.Key.s,
                                     sCount = grp.Count()
                                 };

        List<s_f2_obj> Groupby_f2_S2 = Groupby_f2_S_init2.ToList();

        for (int i = 1; i <= Num_Features; i++)
            for (int k = 1; k <= Num_Senses; k++)
                if (!Groupby_f2_S2.Any(x => x.sf2 == i && x.sID == k))
                    Groupby_f2_S2.Add(new s_f2_obj(i, k, 0));

        //foreach (var t in Groupby_f2_S2)
        //    Response.Write(string.Format("<br/>sID:{0}, sf2:{1}, Count:{2}", t.sID, t.sf2, t.sCount));

        //Prob(f2|s)
        var Prob_f2_Given_s1 = from s in Groupby_f2_S2
                               select new Prob_f2_Given_s_obj { sID = s.sID, sf2 = s.sf2, Prob_f2_Given_s = (float)int.Parse(s.sCount.ToString()) / Groupby_S.Where(x => x.sID == s.sID).First().sCount };

        Prob_f2_Given_s = Prob_f2_Given_s1.ToList();

        Response.Write("<BR/>P(F2|s)");
        foreach (var t in Prob_f2_Given_s.OrderBy(x => x.sID).ThenBy(x => x.sf2))
        {
            Response.Write(string.Format("<br/>Prob(f2={1}|s={0})={2}", t.sID, t.sf2, t.Prob_f2_Given_s));
            Theta_New[Theta_Items_counter++] = t.Prob_f2_Given_s;
        }

        //Now at this stage we have both the values for Theta_New  and Theta_Old ready
        //So let's study the convergence
        for (int i = 0; i < 15; i++)
            Response.Write("<br/>Compare: New:" + Theta_New[i].ToString() + " Vs. Old:" + Theta_Old[i].ToString());

        return Distance_Between_2_Vectors(Theta_New, Theta_Old, 15);
    }
    protected double Distance_Between_2_Vectors(double[] a, double[] b,int ArraySize)
    {   
        double Vectors_difference = 0.0;

        for (int i = 0; i < ArraySize; i++)
            Vectors_difference += Math.Pow((a[i] - b[i]), 2);

            return Vectors_difference;        
    }
    
    
}