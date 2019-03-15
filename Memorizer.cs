using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Memorizer
{
    public class Memorizer
    {
        private Dictionary<Control, List<string>> DMemo = new Dictionary<Control, List<string>>();
        public Control PreviousControl = null;
        private int PosInList = 0;

        public void Set(Control c)
        {
            if (c.Text != "")
            {
                if (!DMemo.ContainsKey(c))
                {
                    List<string> LS = new List<string>();
                    LS.Add(c.Text);
                    DMemo.Add(c, LS);
                }
                else
                {
                    //entrée précédente : 
                    string Previous = DMemo[c][0].ToString();

                    /*
                    //on supprime les entrées précédentes si elles sont moins complètes ou identiques.
                    if (Previous.Length <= c.Text.Length && Previous == c.Text.Substring(0, Previous.Length))
                        DMemo[c].RemoveAt(0);                                            

                    //il faut prévoire les éventuelles corrections.
                    else if (PreviousControl == c)
                        DMemo[c].RemoveAt(0);
                    
                    */
                    if (c.Text != Previous)
                    {
                        DMemo[c].Insert(0, c.Text);

                        PreviousControl = c;

                        //purge au dela de 2000 entrées par control dans le mémo, pour ne pas saturer la ram
                        if (DMemo[c].Count > 2000)
                            DMemo[c].RemoveAt(2001);
                    }
                }
            }
        }

        public void Get(Control c)
        {
            if (DMemo.ContainsKey(c))
            {
                if (PreviousControl == null || PreviousControl != c)
                {
                    PosInList = 0;
                    try { c.Text = DMemo[c][PosInList].ToString(); }
                    catch { }
                }
                else
                {
                    if (PosInList + 1 < DMemo[c].Count)
                    {
                        PosInList++;
                        c.Text = DMemo[c][PosInList].ToString();
                    }

                }
                PreviousControl = c;

                //on a fait le tour du dictionnaire, donc on repart à zero

                if (PosInList + 1 == DMemo[c].Count)
                    PreviousControl = null;
            }
        }

        //à la validation, pas sur que ça serve...
        public void SetAll(Form f)
        {
            foreach (Control c in GetControls(f))
            {
                Set(c);
            }
            PreviousControl = null;
        }

        private IEnumerable<Control> GetControls(Control form)
        {
            foreach (Control childControl in form.Controls)
            {   // Recurse child controls.
                foreach (Control grandChild in GetControls(childControl))
                {
                    yield return grandChild;
                }
                yield return childControl;
            }
        }

        public void Debug()
        {
            string Result = "";

            foreach (var pair in DMemo)
            {
                string S = "";
                foreach (string s in pair.Value)
                {
                    S += s + "\r\n";
                }
                Result += pair.Key.Name + ":" + S + "\r\n\r\n";

            }
            
            MessageBox.Show(Result);
        }
    }
    
}
