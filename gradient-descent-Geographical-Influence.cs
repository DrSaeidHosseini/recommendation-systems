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
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Diagnostics;
public class TuningParamsClass
{
    public double b;
    public double m;
}
public
    partial class algorithms_gradient_descent_geo_influence : System.Web.UI.Page
{
    //This page is the implementation of Gradient Descent to minimize the error function of power law based geographical influence.   

    private double compute_error_for_line_given_points(double b, double m, List<Point> points)
    {
        double totalError = 0.0;
        foreach (Point p in points)
        {
            double pY = double.Parse(p.Y.ToString());
            double pX = double.Parse(p.X.ToString());

            //Remember: y'=logy, b=log a, x'=log x
            pY = Math.Log10(pY);
            pX = Math.Log10(pX);

            double LinearRegVal = pY - (m * pX + b);
            totalError += Math.Pow(LinearRegVal, 2.0);
        }
        return totalError / points.Count();
    }

    private TuningParamsClass step_gradient(double b_current, double m_current, List<Point> points, double learningRate)
    {
        double b_gradient = 0;
        double m_gradient = 0;
        double N = double.Parse(points.Count.ToString());
        foreach (Point p in points)
        {

            //In our function:
            /*
             * y'=logy, b=log a, x'=log x
             * When we found the values for b so the a will be retrieved: 10 power b = a
             * Then we can make our formula: y=a x power m
             * 
             */

            double pY = double.Parse(p.Y.ToString());
            double pX = double.Parse(p.X.ToString());

            //Remember: y'=logy, x'=log x
            pX = Math.Log10(pX);
            pY = Math.Log10(pY);

            b_gradient += -(2 / N) * (pY - ((m_current * pX) + b_current));
            m_gradient += -(2 / N) * pX * (pY - ((m_current * pX) + b_current));
            
        }
        TuningParamsClass tp = new TuningParamsClass();
        tp.b = b_current - (learningRate * b_gradient);
        tp.m = m_current - (learningRate * m_gradient);

        return tp;
    }
    private TuningParamsClass gradient_descent_runner(List<Point> points, double starting_b, double starting_m, double learning_rate, int num_iterations)
    {
        TuningParamsClass tpFinal = new TuningParamsClass();
        double b = starting_b;
        double m = starting_m;

        for (int i = 0; i <= num_iterations; i++)
        {
            litOutput.Text += "<BR/><BR/>Step:" + i.ToString() + ", b: [" + b.ToString() + "] m: [" + m.ToString() + "] " + DateTime.Now.TimeOfDay.ToString();
            Debug.WriteLine("Step:" + i.ToString() + ", b: [" + b.ToString() + "] m: [" + m.ToString() + "] " + DateTime.Now.TimeOfDay.ToString());

            tpFinal = step_gradient(b, m, points, learning_rate);
            b = tpFinal.b;
            m = tpFinal.m;

            litOutput.Text += "<br/><br/>&nbsp;&nbsp;> " + string.Format("error = {0}", compute_error_for_line_given_points(b, m, points));
            Debug.WriteLine("&nbsp;&nbsp;> " + string.Format("error = {0}", compute_error_for_line_given_points(b, m, points)));

        }
        return tpFinal;
    }
}