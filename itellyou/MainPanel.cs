using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace itellyou
{
	public partial class MainPanel : Form
	{
		public MainPanel()
		{
			InitializeComponent();
		}

		private void Form2_Load(object sender, EventArgs e)
		{

		}

		private void button1_Click(object sender, EventArgs e)
		{
			try
			{
				HttpWebRequest request = WebRequest.Create("http://msdn.itellyou.cn/Category/Index") as HttpWebRequest;
				request.Method = "POST";
				request.Headers.Add("Origin", "http://msdn.itellyou.cn/");
				request.Referer = "http://msdn.itellyou.cn/";
				request.UserAgent = XConst.UserAgent;
				request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                Stream formStream = request.GetRequestStream();
				StreamWriter formStreamWriter = new StreamWriter(formStream);
				formStreamWriter.Write(string.Format("id={0}", XConst.OS));
				formStreamWriter.Close();
				formStream.Close();

				HttpWebResponse response = request.GetResponse() as HttpWebResponse;
				var responseStream = response.GetResponseStream();
				StreamReader r = new StreamReader(responseStream);
				string text = r.ReadToEnd();
				r.Close();
				responseStream.Close();

				ArrayList a= MiniJsonExtensions.arrayListFromJson(text);
				foreach ( var ele in a)
				{
					Hashtable item = ele as Hashtable;
					treeView1.Nodes.Add(item["name"] as string);
				}
			}
			catch (Exception ecxept)
			{
			}
		}
	}
}
