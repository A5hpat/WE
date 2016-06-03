using AspDotNetStorefront.Filters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data;
using System.Configuration;
using AspDotNetStorefront.Routing;


namespace AspDotNetStorefront.Controllers
{
    [SecureAccessFilter(forceHttps: true)]
    public class CartController : Controller
    {

        public ActionResult Index()
        {

            return View(ViewNames.CartDemo);
        }

        [HttpPost]
        public ActionResult AddToCart(
            string customerid, string productid, string variantid, string quantity,
            string shippingid, string billingid, string price, string storeid, string returnURL)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;

            try
            {

                conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConn"].ConnectionString);
                conn.Open();

                // 1.  create a command object identifying
                //     the stored procedure
                SqlCommand cmd = new SqlCommand("nalpac_AddToCart", conn);

                // 2. set the command object so it knows
                //    to execute a stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // 3. add parameter to command, which
                //    will be passed to the stored procedure
                cmd.Parameters.Add(new SqlParameter("@CustomerID", customerid));
                cmd.Parameters.Add(new SqlParameter("@ProductID", productid));
                cmd.Parameters.Add(new SqlParameter("@VariantID", variantid));
                cmd.Parameters.Add(new SqlParameter("@Quantity", quantity));
                cmd.Parameters.Add(new SqlParameter("@ShippingAddressID", shippingid));
                cmd.Parameters.Add(new SqlParameter("@BillingAddressID", billingid));
                cmd.Parameters.Add(new SqlParameter("@StoreID", storeid));
                cmd.Parameters.Add(new SqlParameter("@CartType", "0"));

                // declare the SqlDataReader, which is used in
                // both the try block and the finally block

                rdr = cmd.ExecuteReader();
                // iterate through results
                //while (rdr.Read())
                //{



                //    Console.WriteLine("{0}", rdr["name"].ToString());

                //} 
                //var ser = new JavaScriptSerializer();
                //return ser.Serialize(new AddToCartResponseDTO()
                //{
                //    name = "the name"

                //});

                ViewBag.Name = "the name";
                ViewBag.Result = "The Add was successful";
                return View(ViewNames.CartDemo);

            }
            catch (Exception e)
            {
                //Should creat an error partial
                //return e.ToString() + "  error"
                ViewBag.Result = "There was an error!";
                return View(ViewNames.CartDemo);
            }
            finally
            {
                // 3. close the reader
                if (rdr != null)
                {
                    rdr.Close();
                }

                // close the connection
                if (conn != null)
                {
                    conn.Close();
                }
            }

        }

        [Serializable]
        public class AddToCartResponseDTO
        {
            public string name { get; set; }
        }


    }
}
