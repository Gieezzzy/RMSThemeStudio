using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace RMSThemeStudio
{
    public class DbManager
    {
        private readonly AppSettings _settings;

        public DbManager(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task<(bool Success, string Message)> TestConnectionAsync()
        {
            try
            {
                using var conn = new SqlConnection(_settings.GetConnectionString());
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.tbl_MessageThemes", conn);
                var count = await cmd.ExecuteScalarAsync();
                return (true, $"Connected successfully! (Themes in table: {count})");
            }
            catch (Exception ex)
            {
                return (false, "Connection error: " + ex.Message);
            }
        }

        public async Task<List<ThemeModel>> LoadThemesAsync()
        {
            var list = new List<ThemeModel>();
            try
            {
                using var conn = new SqlConnection(_settings.GetConnectionString());
                await conn.OpenAsync();
                string sql = $"SELECT ThemeID, themeName, themebackgroundPath, ReceiverBubble, SenderBubble, DefaultIcon, SetIconGroup, callbackgroudPath, Status, CreatedAt, UpdatedAt, ISNULL(textFontColor, '#FFFFFF') FROM {_settings.TableName} ORDER BY ThemeID ASC";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var m = new ThemeModel
                    {
                        ThemeID = reader.GetInt32(0),
                        ThemeName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        ThemeBackgroundPath = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        ReceiverBubble = reader.IsDBNull(3) ? "#1E293B" : reader.GetString(3),
                        SenderBubble = reader.IsDBNull(4) ? "#0284C7" : reader.GetString(4),
                        DefaultIcon = reader.IsDBNull(5) ? "✨" : reader.GetString(5),
                        SetIconGroup = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        CallBackgroundPath = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        Status = reader.IsDBNull(8) ? "Public" : reader.GetString(8),
                        CreatedAt = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
                        UpdatedAt = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                        TextFontColor = reader.IsDBNull(11) ? "#FFFFFF" : reader.GetString(11)
                    };
                    list.Add(m);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadThemesAsync error: " + ex.Message);
            }
            return list;
        }

        public async Task<(bool Success, string Message)> UpsertThemeAsync(ThemeModel theme)
        {
            try
            {
                using var conn = new SqlConnection(_settings.GetConnectionString());
                await conn.OpenAsync();

                string checkSql = $"SELECT COUNT(*) FROM {_settings.TableName} WHERE ThemeID = @ID";
                using var checkCmd = new SqlCommand(checkSql, conn);
                checkCmd.Parameters.AddWithValue("@ID", theme.ThemeID);
                int exists = (int)await checkCmd.ExecuteScalarAsync();

                if (exists > 0 && theme.ThemeID > 0)
                {
                    string updateSql = $@"UPDATE {_settings.TableName} 
                        SET themeName = @Name,
                            themebackgroundPath = @Bg,
                            ReceiverBubble = @Recv,
                            SenderBubble = @Send,
                            textFontColor = @TextFontColor,
                            DefaultIcon = @Icon,
                            SetIconGroup = @IconGroup,
                            callbackgroudPath = @CallBg,
                            Status = @Status,
                            UpdatedAt = GETDATE()
                        WHERE ThemeID = @ID";

                    using var updateCmd = new SqlCommand(updateSql, conn);
                    updateCmd.Parameters.AddWithValue("@ID", theme.ThemeID);
                    updateCmd.Parameters.AddWithValue("@Name", theme.ThemeName);
                    updateCmd.Parameters.AddWithValue("@Bg", theme.ThemeBackgroundPath);
                    updateCmd.Parameters.AddWithValue("@Recv", theme.ReceiverBubble);
                    updateCmd.Parameters.AddWithValue("@Send", theme.SenderBubble);
                    updateCmd.Parameters.AddWithValue("@TextFontColor", string.IsNullOrWhiteSpace(theme.TextFontColor) ? "#FFFFFF" : theme.TextFontColor);
                    updateCmd.Parameters.AddWithValue("@Icon", theme.DefaultIcon);
                    updateCmd.Parameters.AddWithValue("@IconGroup", theme.SetIconGroup);
                    updateCmd.Parameters.AddWithValue("@CallBg", theme.CallBackgroundPath);
                    updateCmd.Parameters.AddWithValue("@Status", theme.Status);

                    await updateCmd.ExecuteNonQueryAsync();
                    return (true, $"Updated theme '{theme.ThemeName}' (ID {theme.ThemeID}) successfully!");
                }
                else
                {
                    string insertSql = $@"INSERT INTO {_settings.TableName} 
                        (themeName, themebackgroundPath, ReceiverBubble, SenderBubble, textFontColor, DefaultIcon, SetIconGroup, callbackgroudPath, Status, CreatedAt, UpdatedAt)
                        VALUES (@Name, @Bg, @Recv, @Send, @TextFontColor, @Icon, @IconGroup, @CallBg, @Status, GETDATE(), GETDATE());
                        SELECT SCOPE_IDENTITY();";

                    using var insertCmd = new SqlCommand(insertSql, conn);
                    insertCmd.Parameters.AddWithValue("@Name", theme.ThemeName);
                    insertCmd.Parameters.AddWithValue("@Bg", theme.ThemeBackgroundPath);
                    insertCmd.Parameters.AddWithValue("@Recv", theme.ReceiverBubble);
                    insertCmd.Parameters.AddWithValue("@Send", theme.SenderBubble);
                    insertCmd.Parameters.AddWithValue("@TextFontColor", string.IsNullOrWhiteSpace(theme.TextFontColor) ? "#FFFFFF" : theme.TextFontColor);
                    insertCmd.Parameters.AddWithValue("@Icon", theme.DefaultIcon);
                    insertCmd.Parameters.AddWithValue("@IconGroup", theme.SetIconGroup);
                    insertCmd.Parameters.AddWithValue("@CallBg", theme.CallBackgroundPath);
                    insertCmd.Parameters.AddWithValue("@Status", theme.Status);

                    var newIdObj = await insertCmd.ExecuteScalarAsync();
                    if (newIdObj != null && int.TryParse(newIdObj.ToString(), out int newId))
                    {
                        theme.ThemeID = newId;
                    }
                    return (true, $"Created new theme '{theme.ThemeName}' (ID {theme.ThemeID}) in database!");
                }
            }
            catch (Exception ex)
            {
                return (false, "Database save error: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteThemeAsync(int themeId)
        {
            try
            {
                using var conn = new SqlConnection(_settings.GetConnectionString());
                await conn.OpenAsync();
                string sql = $"DELETE FROM {_settings.TableName} WHERE ThemeID = @ID";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", themeId);
                await cmd.ExecuteNonQueryAsync();
                return (true, $"Deleted theme ID {themeId} from database.");
            }
            catch (Exception ex)
            {
                return (false, "Delete error: " + ex.Message);
            }
        }
    }
}
