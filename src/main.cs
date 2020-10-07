using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace 논문분석_도우미
{
    public partial class mainForm : MetroFramework.Forms.MetroForm
    {

		// 단축키를위한 영역 (시작)
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        const int HOTKEY_ID = 31197;
        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8
        }
        const int WM_HOTKEY = 0x0312;
        protected override void WndProc(ref Message message)
        {
            try
            {
                switch (message.Msg)
                {
                    case WM_HOTKEY:
                        Keys key = (Keys)(((int)message.LParam >> 16) & 0xFFFF);
                        KeyModifiers modifier = (KeyModifiers)((int)message.LParam & 0xFFFF);

                        if ((KeyModifiers.Control | KeyModifiers.Shift) == modifier && Keys.B == key)
                        {
                            if (inputTextBox.Text == initTxt) inputTextBox.Clear();
                            inputTextBox.AppendText("\r\n");
                        }

                        if ((KeyModifiers.Control | KeyModifiers.Shift) == modifier && Keys.V == key)
                        {
                            if (inputTextBox.Text == initTxt) inputTextBox.Clear();
                            inputTextBox.AppendText(Clipboard.GetText() + "\r\n");
                        }

                        if ((KeyModifiers.Control | KeyModifiers.Shift) == modifier && Keys.Z == key)
                        {
                            inputTextBox.Text = txt_backup;
                        }

                        if ((KeyModifiers.None) == modifier && Keys.F6 == key)
                        {
                            SendKeys.Send("^c");
                        }
                        if ((KeyModifiers.None) == modifier && Keys.F7 == key)
                        {
                            if (inputTextBox.Text == initTxt) inputTextBox.Clear();
                            inputTextBox.AppendText(Clipboard.GetText() + "\r\n");
                        }
                        if ((KeyModifiers.None) == modifier && Keys.F8 == key)
                        {
                            if (inputTextBox.Text == initTxt) inputTextBox.Clear();
                            inputTextBox.AppendText("\r\n");
                        }
                        break;
                }
                base.WndProc(ref message);
            }
            catch { }
        }





        const string initTxt = "이곳에 내용을 작성해주세요 (문단구분 : 엔터 2번)";
        const int paddSize = 30;
        string txt_backup, txt_backup2;


        List<string> Ptxt1 = new List<string>();
        List<string> Ptxt2 = new List<string>();
        List<int> Pnum = new List<int>();
        List<int> koBlank = new List<int>();

        

		// 문자열 처리를 위한 함수
        private string Parsing(string str, string a, string b)
        {
            if (str.IndexOf(a) > -1)
            {
                if (str.IndexOf(b, str.IndexOf(a) + a.Length) > -1)
                {
                    return str.Substring(str.IndexOf(a) + a.Length, str.IndexOf(b, str.IndexOf(a) + a.Length) - str.IndexOf(a) - a.Length);
                }
            }
            return "";
        }
        private string Parsing(string str, string a)
        {
            if (str.IndexOf(a) > -1)
            {
                return str.Substring(str.IndexOf(a) + a.Length);
            }
            return "";
        }

		// 문자열 A, B의 연결 부분이 알파벳일 경우 true 반환
        private bool AlphabetCheck(string A, string B)
        {
            if (A.Length > 0) {
                char t = A[A.Length - 1];
                if ('a' <= t && t <= 'z') return true;
                if ('A' <= t && t <= 'Z') return true;
                if(t == ')' || t == '(' || t == '.' || t == ',') return true;
            }
            if (B.Length > 0)
            {
                char t = B[0];
                if ('a' <= t && t <= 'z') return true;
                if ('A' <= t && t <= 'Z') return true;
                if (t == ')' || t == '(' || t == '.' || t == ',') return true;
            }
            return false;
        }

		// 띄어쓰기 검사 후 문자열 A, B의 연결 부분이 공백인지 체크
        private int BlankCheck(string str, string A, string B)
        {
            if (B.Trim().Length == 0) return -1;

            int cnt = (A + B).IndexOf(str.Replace(" ", string.Empty));
            if (cnt == -1) return 0;
            if (A.Length < cnt) return 0;

            for (int i = 0; i < str.Length; i++)
            {
                if (cnt == A.Length)
                {
                    if (str[i] == ' ') return 1;
                    return -1;
                }
                else
                {
                    if (str[i] == A[cnt]) cnt++;
                }
            }
            return 0;
        }



		// 띄어쓰기 검사 함수
        private void Spacing()
        {
            if (Ptxt1.Count > 0)
            {
                stateLabel.Text = "띄어쓰기 검사중..";
                Application.DoEvents();
                string t = string.Empty;
                for (int i = 0; i < Ptxt1.Count; i++)
                {
                    t += Ptxt1[i] + Ptxt2[i] + "\r\n";
                }
                try
                {
                    using (StreamWriter outputFile = new StreamWriter(Application.StartupPath + "/Spacing.in"))
                    {
                        outputFile.Write(t);
                    }
                }
                catch { }

                string t1, t2;
                try
                {
                    if (!File.Exists(Application.StartupPath + "/Spacing.in"))
                    {
                        MessageBox.Show("띄어쓰기 검사 파일 생성 오류!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!File.Exists("Spacing.dll"))
                    {
                        MessageBox.Show("Spacing.dll 파일이 존재하지 않습니다!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.FileName = "Spacing.dll";
                    p.Start();
                    p.WaitForExit();

                    stateLabel.Text = "띄어쓰기 검사 완료";
                    Application.DoEvents();

                    string[] output;
                    output = File.ReadAllLines(Application.StartupPath + "/Spacing.out");
                    for (int i = 0; i < output.Length; i++)
                    {
                        t1 = t.Split('\n')[i].Replace("\r", string.Empty);
                        t2 = output[i].Replace("\r", string.Empty);

                        if (AlphabetCheck(Ptxt1[i], Ptxt2[i]))
                        {
                            koBlank.Add(1);
                        }
                        else
                        {
                            koBlank.Add(BlankCheck(t2, Ptxt1[i], Ptxt2[i]));
                        }
                    }
                    File.Delete(Application.StartupPath + "/Spacing.out");
                }
                catch {
					MessageBox.Show("띄어쓰기 검사중 오류가 발생했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
            }
        }


		// 개행문자 두번을 기준으로 문단을 구분
        private void SplitParagraph(string str, int num)
        {

            if (str.IndexOf("\r\n\r\n") > -1)
            {
                string tmp = str.Substring(0, str.IndexOf("\r\n\r\n"));
                string tmp2 = str.Substring(str.IndexOf("\r\n\r\n") + "\r\n\r\n".Length);

                Application.DoEvents();
                SplitParagraph(tmp, num + 1);
                SplitParagraph(tmp2, num + 2);

                return;
            }
            
            if (CheckBox1.Checked)
            {
                int[] idx = new int[3];
                string[] t = new string[3] { "\r\n", "\r", "\n" };


                for (int i = 0; i < t.Length; i++)
                    idx[i] = str.IndexOf(t[i]);



                string t1, t2, t3, t4;
                while (idx[0] > -1 || idx[1] > -1 || idx[2] > -1)
                {
                    int min = -1;
                    for (int i = 0; i < idx.Length; i++)
                    {
                        if (idx[i] > -1 && (min == -1 || idx[min] > idx[i])) min = i;
                    }

                    t1 = str.Substring(0, idx[min]).TrimEnd();
                    t2 = str.Substring(idx[min] + t[min].Length).TrimStart();

                    int[] idx2 = new int[2];
                    string[] tt = new string[2] { " ", "\r\n" };


                    min = -1;
                    for (int i = 0; i < tt.Length; i++)
                        idx2[i] = t1.LastIndexOf(tt[i]);

                    for (int i = 0; i < tt.Length; i++)
                    {
                        if (idx2[i] > -1 && (min == -1 || idx2[min] < idx2[i])) min = i;
                    }
                    if (min > -1)
                    {
                        t3 = t1.Substring(idx2[min] + tt[min].Length);
                    }
                    else
                    {
                        t3 = t1;
                    }

                    min = -1;
                    for (int i = 0; i < tt.Length; i++)
                        idx2[i] = t2.IndexOf(tt[i]);
                    for (int i = 0; i < tt.Length; i++)
                    {
                        if (idx2[i] > -1 && (min == -1 || idx2[min] > idx2[i])) min = i;
                    }
                    if (min > -1)
                    {
                        t4 = t2.Substring(0, idx2[min]);
                        t2 = t2.Substring(idx2[min]);
                    }
                    else
                    {
                        t4 = t2;
                        t2 = string.Empty;
                    }

                    t3 = t3.Replace("\r", string.Empty);
                    t4 = t4.Replace("\r", string.Empty);
                    Ptxt1.Add(t3);
                    Ptxt2.Add(t4);
                    Pnum.Add(num);
                    str = t2;
                    for (int i = 0; i < idx.Length; i++)
                        idx[i] = str.IndexOf(t[i]);

                }
            }
        }

		// 우측 결과 화면에 링크로 변환하는 함수
        private string SpaceBoxFunction(string A, string B, bool blank, bool green, int num, int cnt)
        {
            string s = string.Empty;
            string c = "r";
            string b = string.Empty;

            if (green) c = "g";

            if (A.Length > 0)
                b += A.Substring(A.Length - 1, 1);
            if (blank == false)
                b += " ";
            if (B.Length > 0)
                b += B.Substring(0, 1);

            string tmp = string.Empty;
            if (A.Length > 0)
            {
                s += A.Substring(0, A.Length - 1);
                tmp += A.Substring(A.Length - 1, 1);
            }
            if (blank)
            {
                tmp += " ";
            }
            if (B.Length > 0)
            {
                tmp += B.Substring(0, 1);
            }
            s += "[#*a class=" + c + " href=\"javascript:f('a" + num + "_" + cnt + "','" + b + "', '" + tmp + "')\" id='a" + num + "_" + cnt + "'*#]" + tmp + "[#*/a*#]";
            if (B.Length > 0)
            {
                s += B.Substring(1);
            }
            return s;
        }

        private string strFunction(string str, int num)
        {
            str = str.Trim();
            while (str.IndexOf("\r\n\r\n\r\n") > -1)
                str = str.Replace("\r\n\r\n\r\n", "\r\n\r\n");


            if (CheckBox4.Checked) // 수식 및 특수문자 치환
            {
                string[] org = new string[] { "α", "β", "ε", "τ", "σ", "ω", "Θ", "γ", "μ", "Ω", "ν", "∂",  "δ", "°",  "{", "}", "(", ")", "[", "]", "=", "+", "*", "-", "/", ".", ",", ">", "<", "|", "|",  ";", "ρ", "Σ", "•", "∫", "『", "』", "Ψ", "Π", "ζ", "˙", "λ", "Φ", "Φ", "˙˙", ":", "Δ" };
                string[] frm = new string[] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "󰡔",  "󰡕",  "", "", "", "", "", "", "", "",  "", "" };

                char upCode = '';

                for (int i = 0; i < frm.Length; i++)
                {
                    str = str.Replace(frm[i], org[i]);
                }
                for (int i = 1; i <= 10; i++) // 숫자
                {
                    str = str.Replace(Convert.ToChar(57395 + i).ToString(), (i % 10).ToString());
                }
                for (int i = 0; i < 26; i++) // 알파벳
                {
                    str = str.Replace(Convert.ToChar(57344 + i).ToString(), Convert.ToChar(65 + i).ToString());
                    str = str.Replace(Convert.ToChar(57573 + i).ToString(), Convert.ToChar(97 + i).ToString());
                }


                string str2 = str.Split(upCode)[0];
                for (int i = 1; i < str.Split(upCode).Length; i++)
                {
                    if (str.Split(upCode)[i].TrimStart().Length > 0)
                    {
                        str2 += str.Split(upCode)[i].TrimStart().Substring(0, 1) + Convert.ToChar(771).ToString();
                        str2 += str.Split(upCode)[i].TrimStart().Substring(1);
                        continue;
                    }
                    str2 += str.Split(upCode)[i];
                }

                char breakUnicode = Convert.ToChar(1);

                str = str2;
            }


            if (num == 1 && CheckBox1.Checked)
            {
                Ptxt1.Clear();
                Ptxt2.Clear();
                Pnum.Clear();

                koBlank.Clear();

                SplitParagraph(str, num);
                Spacing();
            }


            if (str.IndexOf("\r\n\r\n") > -1)
            {
                string tmp = str.Substring(0, str.IndexOf("\r\n\r\n"));
                string tmp2 = str.Substring(str.IndexOf("\r\n\r\n") + "\r\n\r\n".Length);

                Application.DoEvents();
                return strFunction(tmp, num + 1) + strFunction(tmp2, num + 2);
            }



            if (CheckBox3.Checked) // <Fig>, <Table>, <Alphabet> 수정
            {
                Regex re = new Regex(@"<[a-zA-Z0-9., ][^<>가-힣]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                MatchCollection resultColl = re.Matches(str);

                string[] vals = re.Split(str);
                string str2 = string.Empty;
                int cnt = 0;
                foreach (string s in vals)
                {
                    str2 += s;

                    if (cnt < resultColl.Count)
                        str2 += (resultColl[cnt].Groups[0]).ToString().Replace("<", "[").Replace(">", "]");
                    cnt++;
                }
                str = str2;
            }


            if (CheckBox2.Checked) // 각주 위첨자 달기 1) 2)
            {

				// ( )괄호가 쌍으로 있을경우 예외처리, 숫자 년도 등
                Regex re2 = new Regex(@"[(][\r\n가-힣A-Za-z0-9=<>＝＜＞：；℃.,;:·⋅∼~≤≥αβß&′*# ×÷±\/\+\-]+[)]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                MatchCollection resultColl2 = re2.Matches(str);

                Regex re = new Regex(@"[0-9,∼~ \-]*[0-9][)]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                MatchCollection resultColl = re.Matches(str);

                string[] vals = re.Split(str);
                string[] vals2 = re2.Split(str);

                string str2 = string.Empty;
                int cnt = 0;
                bool t;
                foreach (string s in vals)
                {
                    str2 += s;

                    if (cnt < resultColl.Count)
                    {
                        t = false;
                        for (int i = 0; i < resultColl2.Count; i++)
                        {
                            if (resultColl2[i].Index <= resultColl[cnt].Index && resultColl[cnt].Index < resultColl2[i].Index + resultColl2[i].Length)
                            {
                                t = true;
                                break;
                            }

                        }

                        if (t || s == string.Empty)
                            str2 += resultColl[cnt].Groups[0];
                        else
                            str2 += "[#*a class=b href=\"javascript:fsup('b" + num + "_" + cnt + "')\" id='b" + num + "_" + cnt + "'*#][#*sup*#]" + resultColl[cnt].Groups[0] + "[#*/sup*#][#*/a*#]";

                    }
                    cnt++;
                }
                str = str2;


                //각주 [1], [2] 처리
                Regex re3 = new Regex(@"\[[0-9,∼~ \-]*[0-9]\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                MatchCollection resultColl3 = re3.Matches(str);

                string[] vals3 = re3.Split(str);

                str2 = string.Empty;
                cnt = 0;
                foreach (string s in vals3)
                {
                    str2 += s;

                    if (cnt < resultColl3.Count)
                    {
                        if (s == string.Empty)
                            str2 += resultColl3[cnt].Groups[0];
                        else
                            str2 += "[#*a class=b href=\"javascript:fsup('b" + num + "_" + cnt + "')\" id='b" + num + "_" + cnt + "'*#]" + resultColl3[cnt].Groups[0] + "[#*/a*#]";
                    }
                    cnt++;
                }
                str = str2;
                
            }




            if (CheckBox1.Checked)
            {
                int[] idx = new int[3];
                string[] t = new string[3] { "\r\n", "\r", "\n" };


                for (int i = 0; i < t.Length; i++)
                    idx[i] = str.IndexOf(t[i]);

                string txt = string.Empty;

                int cnt = 0;
                for (int i = 0; i < Pnum.Count; i++)
                {
                    if (Pnum[i] == num)
                    {
                        cnt = i;
                        break;
                    }
                }
                string t1, t2, t3, t4;
                while (idx[0] > -1 || idx[1] > -1 || idx[2] > -1)
                {
                    int min = -1;
                    for (int i = 0; i < idx.Length; i++)
                    {
                        if (idx[i] > -1 && (min == -1 || idx[min] > idx[i])) min = i;
                    }

                    t1 = str.Substring(0, idx[min]).TrimEnd();
                    t2 = str.Substring(idx[min] + t[min].Length).TrimStart();

                    int[] idx2 = new int[6];
                    string[] tt = new string[6] { " ", "*#]", "[#*", ">", "<", "\r\n" };


                    min = -1;
                    for (int i = 0; i < tt.Length; i++)
                        idx2[i] = t1.LastIndexOf(tt[i]);
                    for (int i = 0; i < tt.Length; i++)
                    {
                        if (idx2[i] > -1 && (min == -1 || idx2[min] < idx2[i])) min = i;
                    }
                    if (min > -1)
                    {
                        t3 = t1.Substring(idx2[min] + tt[min].Length);
                        t1 = t1.Substring(0, idx2[min] + tt[min].Length);
                    }
                    else
                    {
                        t3 = t1;
                        t1 = string.Empty;
                    }



                    min = -1;
                    for (int i = 0; i < tt.Length; i++)
                        idx2[i] = t2.IndexOf(tt[i]);
                    for (int i = 0; i < tt.Length; i++)
                    {
                        if (idx2[i] > -1 && (min == -1 || idx2[min] > idx2[i])) min = i;
                    }
                    if (min > -1)
                    {
                        t4 = t2.Substring(0, idx2[min]);
                        t2 = t2.Substring(idx2[min]);
                    }
                    else
                    {
                        t4 = t2;
                        t2 = string.Empty;
                    }
                    txt += t1;

                    int grade = koBlank[cnt];

                    if (t4.Length > 0)
                    {
                        if (grade > 0)
                        {
                            txt += SpaceBoxFunction(t3, t4, true, true, num, cnt);
                        }
                        else
                        {
                            txt += SpaceBoxFunction(t3, t4, false, true, num, cnt);
                        }
                    }
                    else
                    {
                        txt += t3;
                    }


                    str = t2;

                    for (int i = 0; i < idx.Length; i++)
                        idx[i] = str.IndexOf(t[i]);
                    cnt++;
                }
                txt += str;

                str = txt;
            }

            str = "<span ondblclick=\"fstrong('s" + num + "')\" id='s" + num + "'>" + str + "</span>"; // 두번 클릭하여 strong
            str = "<p>" + str + "</p>";
            return str.Replace("[#*", "<").Replace("*#]", ">");
        }





        private void ok_confirm()
        {
			// 저장 버튼
            try
            {
                string str = resultBrowser.Document.Body.InnerHtml;

                if (str != null)
                {
                    str = str.Replace("&nbsp;", " ");
                    str = Regex.Replace(str, @"<(\/[Aa]|[Aa])([^>]*)>", string.Empty);
                    str = Regex.Replace(str, @"<(\/[Ss][Pp][Aa][Nn]|[Ss][Pp][Aa][Nn])([^>]*)>", string.Empty);

                    resultBrowser.Document.Body.SetAttribute("contentEditable", "true");
                    resultBrowser.Document.Body.InnerHtml = str;

                    stateLabel.Text = "저장 완료";
                    Application.DoEvents();
                }
                else
                {
                    stateLabel.Text = "저장할 내용이 없습니다.";
                    Application.DoEvents();
                }
            }
            catch { }
        }
        private void btn_process()
        {
			// 변환 버튼
            processButton.Enabled = false;
            stateLabel.Text = "처리 중..";
            Application.DoEvents();
            resultBrowser.Navigate("about:blank");
            try
            {
				// 변환 전 backup.txt 파일을 생성
                using (StreamWriter outputFile = new StreamWriter(Application.StartupPath + "/backup.txt"))
                {
                    outputFile.Write(inputTextBox.Text);
                }
            }
            catch {
                MessageBox.Show("백업 파일을 생성할수 없습니다!!\r\n폴더 권한 문제나 backup.txt 파일이 사용 중 일 경우 오류가 발생할 수 있습니다.\r\n\r\n오류가 계속된다면 해당 본문 내용을 메일로 보내주시면 수정하겠습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            string str = string.Empty;
            try
            {
                using (StreamWriter outputFile = new StreamWriter(Application.StartupPath + "/data.html"))
                {
					// data.html로 저장하여 불러오기
                    str = "<meta charset=utf-8><style>body{font-family:'Malgun Gothic';}p{line-height: 1.5em;}a{text-decoration:none;}a:hover{font-weight:bold;text-decoration:underline;}span{cursor:default;}.r{color:red;border:1px solid red;}.g{color:green;border:1px solid green;}.p{color:purple;}.b{color:blue;}</style><script>function f(idx,txt,txt2){if(document.getElementById(idx).innerHTML.toLowerCase()==txt2.toLowerCase()){document.getElementById(idx).innerHTML=txt;}else{document.getElementById(idx).innerHTML=txt2;}}function fstrong(idx){if(document.getElementById(idx).innerHTML.search(new RegExp('<strong>', 'i'))==-1){document.getElementById(idx).innerHTML='<strong>'+document.getElementById(idx).innerHTML+'</strong>';}else{document.getElementById(idx).innerHTML=document.getElementById(idx).innerHTML.replace(/<(\\/strong|strong)([^>]*)>/gi,'');}}function fsup(idx){if(document.getElementById(idx).innerHTML.search(new RegExp('<sup>', 'i'))==-1){document.getElementById(idx).innerHTML='<sup>'+document.getElementById(idx).innerHTML+'</sup>';}else{document.getElementById(idx).innerHTML=document.getElementById(idx).innerHTML.replace(/<(\\/sup|sup)([^>]*)>/gi,'');}}</script><body>" + strFunction(inputTextBox.Text, 1) + "</body>";
                    outputFile.Write(str);
                }
                resultBrowser.Navigate(Application.StartupPath + "/data.html");
            }
            catch
            {
				// data.html로 저장에 실패할경우 webBrowser에 바로 로드
                resultBrowser.Document.Write(str);
            }
            stateLabel.Text = "처리 완료 (저장 버튼을 눌러 주세요)";

            processButton.Enabled = true;
            Application.DoEvents();
        }






        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
			// 폼 종료시 단축키 해제
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            UnregisterHotKey(this.Handle, HOTKEY_ID + 1);
            UnregisterHotKey(this.Handle, HOTKEY_ID + 2);
            UnregisterHotKey(this.Handle, HOTKEY_ID + 3);
            UnregisterHotKey(this.Handle, HOTKEY_ID + 4);
            UnregisterHotKey(this.Handle, HOTKEY_ID + 5);
        }

        private void inputTextBox_Click(object sender, EventArgs e)
        {
            if (inputTextBox.Text == initTxt) inputTextBox.Clear();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("좌측 텍스트 내용을 모두 지우시겠습니까?", "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                inputTextBox.Clear();
            }
        }        

        private void processButton_Click(object sender, EventArgs e)
        {
            btn_process();
        }

        private void inputTextBox_TextChanged(object sender, EventArgs e)
        {
            txt_backup = txt_backup2;
            txt_backup2 = inputTextBox.Text;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            ok_confirm();
        }


        public mainForm()
        {
            InitializeComponent();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
			// 메인 폼 로드
            resultBrowser.Navigate("about:blank");

            inputTextBox.Text = initTxt;
            inputTextBox.Enabled = false;
            clearButton.Enabled = false;
            processButton.Enabled = false;
            saveButton.Enabled = false;
            stateLabel.Text = "잠시만 기다려주세요. 창 이동 시 오류가 발생할 수 있습니다.";

            this.Visible = true;
            Application.DoEvents();


            stateLabel.Text = string.Empty;
            inputTextBox.Enabled = true;
            clearButton.Enabled = true;
            processButton.Enabled = true;
            saveButton.Enabled = true;
            resultBrowser.Navigate("https://github.com/johun204/kisti-study-help/blob/main/README.md#readme");
            Application.DoEvents();


			// 단축키 등록
            RegisterHotKey(this.Handle, HOTKEY_ID, KeyModifiers.Control | KeyModifiers.Shift, Keys.B);
            RegisterHotKey(this.Handle, HOTKEY_ID + 1, KeyModifiers.Control | KeyModifiers.Shift, Keys.V);
            RegisterHotKey(this.Handle, HOTKEY_ID + 2, KeyModifiers.Control | KeyModifiers.Shift, Keys.Z);
            RegisterHotKey(this.Handle, HOTKEY_ID + 3, KeyModifiers.None, Keys.F6);
            RegisterHotKey(this.Handle, HOTKEY_ID + 4, KeyModifiers.None, Keys.F7);
            RegisterHotKey(this.Handle, HOTKEY_ID + 5, KeyModifiers.None, Keys.F8);
        }

        private void mainForm_Resize(object sender, EventArgs e)
        {
			// 폼 크기 변경시 컨트롤 크기 변경
            try
            {

                inputTextBox.Width = (this.Width - paddSize * 3) / 2;
                resultBrowser.Width = inputTextBox.Width;

                inputTextBox.Height = (this.Height - inputTextBox.Top - paddSize);
                resultBrowser.Height = inputTextBox.Height;


                processButton.Location = new Point(inputTextBox.Left + inputTextBox.Width + paddSize, processButton.Location.Y);
                saveButton.Location = new Point(inputTextBox.Left + inputTextBox.Width + paddSize + processButton.Width + paddSize, saveButton.Location.Y);
                resultBrowser.Location = new Point(inputTextBox.Left + inputTextBox.Width + paddSize, resultBrowser.Location.Y);
            }
            catch {
            }
        }

        private void CheckBox6_CheckStateChanged(object sender, EventArgs e)
        {
			// 폼 항상 위
            this.TopMost = CheckBox6.Checked;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
			// 프로그램 내부 단축키
            if (keyData == (Keys.Control | Keys.D))
            {
                btn_process();
                return true;
            }
            if (keyData == (Keys.Control | Keys.R))
            {
                if (MessageBox.Show("좌측 텍스트 내용을 모두 지우시겠습니까?", "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    inputTextBox.Clear();
                }
                return true;
            }
            if (keyData == (Keys.Control | Keys.S))
            {
                ok_confirm();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
