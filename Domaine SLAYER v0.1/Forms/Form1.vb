Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.IO
Imports System.Net

Public Class Form1

    Public URI As String
    ReadOnly DataItems As New Dictionary(Of WebClient, ListViewItem)

    Public Sub New()
        InitializeComponent()
        CheckForIllegalCrossThreadCalls = False
        txtLocal.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 Or SecurityProtocolType.Tls12
    End Sub

    Private Sub Btn_DOwnload_Click(sender As Object, e As EventArgs) Handles Btn_DOwnload.Click
        URI = txturl.Text
        LvLinks.Items.Clear()
        Dim Grab As New Thread(New ThreadStart(Sub() GetDOmaine(URI)))
        Grab.Start()
    End Sub

    Public Sub GetDOmaine(ByVal url As String)
        Try
            Dim X As New Req
            X.SetURI(url)
            AddHandler X.RequestComplete, AddressOf X_ScannerCompleted
            X.Requ()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub X_ScannerCompleted(sender As Object, e As String)
        Try
            File.WriteAllText((Me.txtLocal.Text & "\index.html"), e)
            GetAllTypes(e, "<(link|img|script).+(href|src|class)=""(.*?)"".*>")
            GetAllTypes(e, "<(link|img|script).?(href|src|class)=""(.*?)"".*>")
            Removeduplicates(LvLinks)
            MsgBox("You Can Grab All Pages Now " & vbCrLf & "Click and Grab All Pages", MsgBoxStyle.Information)
            Getpages.Enabled = True
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub GetAllTypes(e As String, pattern As String)
        For Each o As Match In New Regex(pattern).Matches(e)
            Dim NewItem As New ListViewItem With {
                .UseItemStyleForSubItems = False
            }

            Dim D As String = ""

            If o.Groups(3).Value.StartsWith("/") Then
                D = o.Groups(3).Value.Remove(0, 1)
            Else
                D = o.Groups(3).Value
            End If

            NewItem.Tag = D

            If URI.EndsWith("/") Then
                NewItem.Text = URI & D
            Else
                NewItem.Text = URI & "/" & D
            End If
            NewItem.SubItems(0).Font = New Font("Segoe UI Semibold", 8.25, FontStyle.Underline Or FontStyle.Bold)
            NewItem.SubItems(0).ForeColor = Color.Blue
            NewItem.SubItems.Add("Idle.").ForeColor = Color.Blue
            LvLinks.Items.Add(NewItem)
        Next
    End Sub

    Private Sub BtnBrowser_Click(sender As Object, e As EventArgs) Handles btnBrowser.Click
        Dim I As New FolderBrowserDialog With {
            .ShowNewFolderButton = True,
            .RootFolder = Environment.SpecialFolder.Desktop,
            .Description = "Save"
        }
        If I.ShowDialog = Windows.Forms.DialogResult.OK Then
            txtLocal.Text = I.SelectedPath
        End If
    End Sub

#Region "Download Files"
    Public Sub DownloadFile(ByVal NewItem As ListViewItem, ByVal URL As Uri, ByVal Directory As String, ByVal Name As String)
        On Error Resume Next

        Using I As New WebClient

                AddHandler I.DownloadProgressChanged, AddressOf Progress

                AddHandler I.DownloadFileCompleted, AddressOf Completed

                I.DownloadFileTaskAsync(URL, Directory & Name)

                DataItems.Add(I, NewItem)

            End Using


    End Sub

    Private Sub Progress(ByVal R As Object, ByVal e As DownloadProgressChangedEventArgs)
        On Error Resume Next
        LvLinks.Items(DataItems(R).Index).SubItems(1).ForeColor = Color.Gold
        LvLinks.Items(DataItems(R).Index).SubItems(1).Text = "Getting Data.."
    End Sub

    Private Sub Completed(ByVal R As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs)
        Try
            LvLinks.Items(DataItems(R).Index).SubItems(1).ForeColor = Color.Green
            LvLinks.Items(DataItems(R).Index).SubItems(1).Text = "Done !"
        Catch ex As Exception
            LvLinks.Items(DataItems(R).Index).SubItems(1).ForeColor = Color.Red
            LvLinks.Items(DataItems(R).Index).SubItems(1).Text = "Error !"
        End Try
    End Sub

    Public Function Determine(ByVal name As String) As Boolean
        Try
            If Regex.IsMatch("jpg|jpeg|gif|png|jpg|jpeg|gif|png|svg", name.Split(".")(1).ToLower()) Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Return vbNull
        End Try
    End Function

    Public Sub GetFiles(ByVal NewItem As ListViewItem, ByVal URL As String)
        Try

            Using I As New WebClient

                AddHandler I.DownloadProgressChanged, AddressOf Progress
                AddHandler I.DownloadStringCompleted, AddressOf Finish
                I.DownloadStringAsync(New Uri(URL))
                DataItems.Add(I, NewItem)

            End Using

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub Finish(sender As Object, e As DownloadStringCompletedEventArgs)
        On Error Resume Next
        File.WriteAllText((LvLinks.Items(DataItems(sender).Index).ToolTipText.Split("|")(0) & "\" & LvLinks.Items(DataItems(sender).Index).ToolTipText.Split("|")(1)), e.Result)
        LvLinks.Items(DataItems(sender).Index).SubItems(1).ForeColor = Color.Green
        LvLinks.Items(DataItems(sender).Index).SubItems(1).Text = "Done !"
    End Sub

    Public Sub Removeduplicates(ByRef lv As ListView)
        Dim MyItems As New ArrayList
        For Each MyItem As ListViewItem In lv.Items
            If MyItems.Contains(MyItem.Text) Then
                MyItem.Remove()
            Else
                MyItems.Add(MyItem.Text)
            End If
        Next
        For Each itm As ListViewItem In lv.Items
            If itm.Text.Contains("/http") Then
                itm.Remove()
            End If
        Next
    End Sub

#End Region

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Getpages.Click


        If LvLinks.Items.Count = 0 Then
            Exit Sub
        End If

        For Each I As ListViewItem In LvLinks.Items
            Dim ST() As String = I.Tag.ToString.Split("/")
            Dim Index As Integer = 0S
            Dim Name As String = ""
            Dim Path As String = ""
            For Each S As String In ST
                Index += 1
                Path += S & "/"
            Next
            Name = "/" & ST(Index - 1)
            If Index <> 1 Then
                Path = txtLocal.Text & "\" & Path.Replace(Name, Nothing)
            Else
                Path = txtLocal.Text
            End If
            If Determine(Name) Then
                If Not Directory.Exists(Path) Then
                    Directory.CreateDirectory(Path)
                End If
                DownloadFile(I, New Uri(I.Text), Path, Name)
            Else
                If Not Directory.Exists(Path) Then
                    Directory.CreateDirectory(Path)
                End If
                I.ToolTipText = Path & "|" & Name
                Dim Grab As New Thread(New ThreadStart(Sub() GetFiles(I, I.Text)))
                Grab.Start()
            End If

        Next
    End Sub

    Private Sub ToolStripStatusLabel1_Click(sender As Object, e As EventArgs) Handles ToolStripStatusLabel1.Click
        aboutCoder.ShowDialog()
    End Sub
End Class
