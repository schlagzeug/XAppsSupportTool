using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using XCLBCLAIMEDITORHCFALib;
using XCLBCLAIMEDITORUB92Lib;
using System.Windows.Forms;
using XactiMed;

namespace XAppsSupport
{
    class ClaimEditor
    {
        XCLBCLAIMEDITORUB92Lib.ClaimEditorUB92 ub92Editor;
        XCLBCLAIMEDITORHCFALib.ClaimEditorHCFA hcfaEditor;
        ArrayList XMB_List = new ArrayList();
        int m_Index = 0;
        private int m_numEditor;
        int SiteID { get; set; }

        public ClaimEditor(int siteID, int claimType, ArrayList xmbList)
        {
            XMB_List = xmbList;
            string claimXML = XMB_List[0].ToString();
            SiteID = siteID;
            if (claimType == 0)
            {
                OpenUB92(claimXML);
            }
            else if (claimType == 1)
            {
                OpenHCFA(claimXML);
            }
        }

        private void OpenUB92(string sClaimXml)
        {
            try
            {
                if (ub92Editor != null && ub92Editor.IsOpen)
                {
                    ub92Editor.Close(true);
                }
                ub92Editor = new ClaimEditorUB92Class();
                ub92Editor.Mode = XCLBCLAIMEDITORUB92Lib.ClaimEditorMode.CEMODE_STANDALONE_NEW;
                ub92Editor.SiteID = SiteID;
                ub92Editor.ClaimXml = sClaimXml;
                ub92Editor.Open();
                ub92Editor.OnEditorEvent += new XCLBCLAIMEDITORUB92Lib.IClaimEditorClient_OnEditorEventEventHandler(ub92Editor_OnEditorEvent);
                ub92Editor.OnEditorResult += new XCLBCLAIMEDITORUB92Lib.IClaimEditorClient_OnEditorResultEventHandler(ub92Editor_OnEditorResult);
                ub92Editor.Show(true);
            }
            catch (Exception ex)
            {
                Tools.ShowError(ex.ToString());
            }
        }
        private void CloseUB92()
        {
            try
            {
                if (ub92Editor.IsOpen)
                {
                    ub92Editor.Close(true);
                    
                }
            }
            catch (Exception ex)
            {
                Tools.ShowError(ex.ToString());
            }
        }
        private void OpenHCFA(string sClaimXml)
        {
            try
            {
                if (hcfaEditor != null && hcfaEditor.IsOpen)
                {
                    hcfaEditor.Close(true);
                }
                hcfaEditor = new ClaimEditorHCFAClass();
                hcfaEditor.Mode = XCLBCLAIMEDITORHCFALib.ClaimEditorMode.CEMODE_STANDALONE_NEW;
                hcfaEditor.SiteID = SiteID;
                hcfaEditor.ClaimXml = sClaimXml;
                hcfaEditor.Open();

                hcfaEditor.OnEditorEvent += new XCLBCLAIMEDITORHCFALib.IClaimEditorClient_OnEditorEventEventHandler(hcfaEditor_OnEditorEvent);
                hcfaEditor.OnEditorResult += new XCLBCLAIMEDITORHCFALib.IClaimEditorClient_OnEditorResultEventHandler(hcfaEditor_OnEditorResult);

                hcfaEditor.Show(true);

            }
            catch (Exception ex)
            {
                Tools.ShowError(ex.ToString());
            }
        }
        private void CloseHCFA()
        {
            try
            {
                if (hcfaEditor.IsOpen)
                {
                    hcfaEditor.Close(true);
                }
            }
            catch (Exception ex)
            {
                Tools.ShowError(ex.ToString());
            }
        }
        private void ub92Editor_OnEditorEvent(int nTag, XCLBCLAIMEDITORUB92Lib.ClaimEditorEvent nEvent, int nFlags)
        {
            int nEndFile = XMB_List.Count - 1;

            if (nEvent == XCLBCLAIMEDITORUB92Lib.ClaimEditorEvent.CEEVENT_CLOSED && m_Index == nEndFile)
            {
                CloseUB92();
                m_numEditor--;
            }
            if (nEvent == XCLBCLAIMEDITORUB92Lib.ClaimEditorEvent.CEEVENT_ENDEDIT && m_Index == nEndFile)
            {
                CloseUB92();
            }
        }
        private void ub92Editor_OnEditorResult(int nTag, XCLBCLAIMEDITORUB92Lib.ClaimEditorResult nEvent, int nFlags)
        {
            if (nEvent == XCLBCLAIMEDITORUB92Lib.ClaimEditorResult.CERESULT_EDIT_CANCEL ||
                nEvent == XCLBCLAIMEDITORUB92Lib.ClaimEditorResult.CERESULT_VIEW_CANCEL)
            {
                m_Index = m_Index + 1;
                if (m_Index < XMB_List.Count)
                {
                    OpenUB92(XMB_List[m_Index].ToString());
                }
                else
                {
                    Tools.ShowMessage("End of XMB file");
                }
            }
            else if (nEvent == XCLBCLAIMEDITORUB92Lib.ClaimEditorResult.CERESULT_EDIT_CHECKIN)
            {
                string s = ub92Editor.ClaimXml;
                CloseUB92();
                m_numEditor--;
                CreateXmbFile(s, "UB92");
            }
            else if (nEvent == XCLBCLAIMEDITORUB92Lib.ClaimEditorResult.CERESULT_EDIT_CANCEL_ALL ||
                     nEvent == XCLBCLAIMEDITORUB92Lib.ClaimEditorResult.CERESULT_VIEW_CANCEL_ALL)
            {
                CloseUB92();
            }
        }
        private void hcfaEditor_OnEditorEvent(int nTag, XCLBCLAIMEDITORHCFALib.ClaimEditorEvent nEvent, int nFlags)
        {
            int nEndFile = XMB_List.Count - 1;

            if (nEvent == XCLBCLAIMEDITORHCFALib.ClaimEditorEvent.CEEVENT_CLOSED && m_Index == nEndFile)
            {
                CloseHCFA();
                m_numEditor--;
            }
            if (nEvent == XCLBCLAIMEDITORHCFALib.ClaimEditorEvent.CEEVENT_ENDEDIT && m_Index == nEndFile)
            {
                CloseHCFA();
            }
        }
        private void hcfaEditor_OnEditorResult(int nTag, XCLBCLAIMEDITORHCFALib.ClaimEditorResult nEvent, int nFlags)
        {
            if (nEvent == XCLBCLAIMEDITORHCFALib.ClaimEditorResult.CERESULT_EDIT_CANCEL)
            {
                m_Index = m_Index + 1;
                if (m_Index < XMB_List.Count)
                {
                    OpenHCFA(XMB_List[m_Index].ToString());
                }
                else
                {
                    Tools.ShowMessage("End of XMB file");
                }
            }
            else if (nEvent == XCLBCLAIMEDITORHCFALib.ClaimEditorResult.CERESULT_EDIT_CHECKIN)
            {
                if (hcfaEditor.IsOpen)
                {
                    string s = hcfaEditor.ClaimXml;
                    CloseHCFA();
                    m_numEditor--;
                    CreateXmbFile(s, "HCFA1500");
                }
            }
            else if (nEvent == XCLBCLAIMEDITORHCFALib.ClaimEditorResult.CERESULT_EDIT_CANCEL_ALL)
            {
                CloseHCFA();
            }
        }
        private void CreateXmbFile(string sClaimXml, string sType)
        {
            System.Windows.Forms.SaveFileDialog saveDiag = new System.Windows.Forms.SaveFileDialog();
            saveDiag.InitialDirectory = @"C:\CustomerSS\" + SiteID.ToString() + @"\XClaim\" + sType + @"\IMPORT";
            saveDiag.Filter = "XMB File | *.xmb";
            saveDiag.DefaultExt = "xmb";
            saveDiag.FileName = "Test.xmb";
            DialogResult result = saveDiag.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string s = saveDiag.FileName;

                XmlBatchFileWriter _XmlWriter = new XmlBatchFileWriter(@s);
                if (sClaimXml != null && sClaimXml != string.Empty)
                {
                    _XmlWriter.AddDocumentStr(sClaimXml);
                }
                _XmlWriter.Close();
                string message = s + " has been created";
                Tools.ShowMessage(message);
            }
            else
            {
                Tools.ShowMessage("XMB file not created");
            }

        }
    }
}
