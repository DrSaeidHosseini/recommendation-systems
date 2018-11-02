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
Gradient Descent to compute the Geographical Influence Location POI recommendation.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class algorithms_GIStats : System.Web.UI.Page
{
    public string strDays = "";
    public string strViews = "", strViews2 = "";
    public string strMonthz = "";
    public string strMonthly = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        DailyViews();
    }

    private void DailyViews()
    {
        Response.Write("<h2>logY = b + m * LogX</h2>");

        double b = -0.00322556055920203;
        double m = -0.634086835000805;
        string strType = "All pairs";

        SampleData_PowerLaw(b, m, strType);

        b = -0.143599682705936;
        m = -0.349273676463755;
        strType = "Temporal";

        SampleData_PowerLaw(b, m, strType);

    }

    private void SampleData_PowerLaw(double b, double m, string strType)
    {
        //Response.Write("<h3>Experiment 1: " + strType + ", learning Rate=0.1, b = " + b.ToString() + ", m = " + m.ToString() + "</h3>");

        for (int i = 5; i < 10000; i = i + 10)
        {
            double logy = (b + (m * Math.Log10(i)));
            double y = Math.Pow(10.0, logy);
            //Response.Write("<br/>" + i.ToString() + "  ====> " + y.ToString());

            if (strType == "All pairs")
            {
                if (strDays == "")
                    strDays = "'" + General.StringValue(i) + "'";
                else
                    strDays += ",'" + General.StringValue(i) + "'";

                if (strViews == "")
                    strViews = General.StringValue(y);
                else
                    strViews += "," + General.StringValue(y);
            }
            else
            {
               /* if (strDays == "")
                    strDays = "'" + General.StringValue(i) + "'";
                else
                    strDays += ",'" + General.StringValue(i) + "'";*/

                if (strViews2 == "")
                    strViews2 = General.StringValue(y);
                else
                    strViews2 += "," + General.StringValue(y);
            }

        }
    }

    

}