Imports xNet

Public Class Req

    Private URI As String
    Public Event RequestComplete(sender As Object, Result As String)

    Public Sub Requ()
        Try

            Using request As New HttpRequest
                request.Proxy = Nothing
                request.UserAgent = Http.ChromeUserAgent
                request.KeepAlive = True
                request.Cookies = New CookieDictionary(False)
                request.IgnoreProtocolErrors = True
                request.AllowAutoRedirect = False

                Dim Cookies As String = request.Get(Me.URI).Cookies.ToString()

                request.AddHeader("Upgrade-Insecure-Requests", "1")
                request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36")
                request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q")
                request.AddHeader("Accept-Language", "en-US,en;q")

                If Not Cookies = String.Empty Then
                    request.AddHeader("Cookie", Cookies)
                End If

                Dim rep As HttpResponse = request.Get(Me.URI)
                Dim Response As String = ""

                If rep.HasRedirect Then
                    Response = request.Get(rep.RedirectAddress.ToString(), Nothing).ToString()
                Else
                    Response = rep.ToString()
                End If

                RaiseEvent RequestComplete(Me, Response)

            End Using
        Catch ex As Exception
            MsgBox(ex.Message)
            RaiseEvent RequestComplete(Me, ex.Message)
        End Try
    End Sub

    Public Sub SetURI(ByVal URI As String)
        Me.URI = URI
    End Sub
End Class
