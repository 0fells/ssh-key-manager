using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SshKeyManager
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += delegate(object s, ThreadExceptionEventArgs e) { ShowFatal(e.Exception); };
            AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e) { ShowFatal(e.ExceptionObject as Exception); };
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static void ShowFatal(Exception ex)
        {
            try
            {
                MessageBox.Show(L.F("fatal", ex == null ? "?" : ex.Message), "SSH Key Manager",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
        }
    }

    static class L
    {
        public static volatile string Lang = DefaultLang();
        static readonly Dictionary<string, string[]> table = new Dictionary<string, string[]>();

        static string DefaultLang()
        {
            return System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? "ru" : "en";
        }

        public static string T(string key)
        {
            string[] v;
            if (!table.TryGetValue(key, out v)) return key;
            return Lang == "ru" ? v[0] : v[1];
        }

        public static string F(string key, params object[] args)
        {
            return string.Format(T(key), args);
        }

        static void A(string key, string ru, string en)
        {
            table[key] = new[] { ru, en };
        }

        static L()
        {
            A("fatal",
              "Непредвиденная ошибка: {0}",
              "Unexpected error: {0}");
            A("btnRefresh",
              "Обновить",
              "Refresh");
            A("colName",
              "Имя",
              "Name");
            A("colPriv",
              "Приватный",
              "Private");
            A("colFinger",
              "Отпечаток",
              "Fingerprint");
            A("yes",
              "да",
              "yes");
            A("no",
              "нет",
              "no");
            A("btnNew",
              "Создать ключ",
              "Create key");
            A("btnDel",
              "Удалить",
              "Delete");
            A("btnDelAll",
              "Удалить все",
              "Delete all");
            A("btnCopy",
              "Копировать pub",
              "Copy pub");
            A("grpServer",
              "Сервер",
              "Server");
            A("lblUser",
              "Логин",
              "Login");
            A("lblPort",
              "Порт",
              "Port");
            A("lblIp",
              "IP сервера (можно указать несколько через запятую)",
              "Server IP (several allowed, comma-separated)");
            A("btnAddIp",
              "Добавить",
              "Add");
            A("btnRemoveIp",
              "Убрать",
              "Remove");
            A("lblList",
              "Список IP серверов (Ctrl — несколько):",
              "Server IP list (Ctrl for several):");
            A("btnUp",
              "Залить и проверить",
              "Upload and verify");
            A("btnCheck",
              "Только проверить",
              "Verify only");
            A("btnRevoke",
              "Убрать с сервера",
              "Remove from server");
            A("btnCmd",
              "Команда входа",
              "Login command");
            A("keysLabel",
              "Ключи: {0}",
              "Keys: {0}");
            A("busyWait",
              "Дождитесь завершения текущей операции.",
              "Wait for the current operation to finish.");
            A("noOpenSsh",
              "OpenSSH Client не найден. Параметры Windows → Дополнительные компоненты → OpenSSH Client.",
              "OpenSSH Client not found. Windows Settings → Optional features → OpenSSH Client.");
            A("settingsReadFail",
              "Не удалось прочитать настройки {0}: {1}",
              "Failed to read settings {0}: {1}");
            A("fingerFail",
              "Не удалось получить отпечаток {0}: {1}",
              "Failed to get fingerprint of {0}: {1}");
            A("readDirFail",
              "Не удалось прочитать {0}: {1}",
              "Failed to read {0}: {1}");
            A("nameChars",
              "Имя: только буквы, цифры, точка, _ и -; начинается с буквы или цифры.",
              "Name: only letters, digits, dot, _ and -; must start with a letter or digit.");
            A("nameDot",
              "Имя не может заканчиваться точкой.",
              "The name cannot end with a dot.");
            A("namePub",
              "Имя не может заканчиваться на .pub (укажите имя без расширения).",
              "The name cannot end with .pub (enter the name without extension).");
            A("nameReserved",
              "Имя \"{0}\" занято служебным файлом SSH. Выберите другое.",
              "The name \"{0}\" is used by an SSH system file. Choose another one.");
            A("nameDevice",
              "Имя \"{0}\" зарезервировано Windows. Выберите другое.",
              "The name \"{0}\" is reserved by Windows. Choose another one.");
            A("titleError",
              "Ошибка",
              "Error");
            A("noKeygen",
              "Не найден ssh-keygen. Установите OpenSSH Client.",
              "ssh-keygen not found. Install OpenSSH Client.");
            A("dlgTitle",
              "Новый SSH-ключ",
              "New SSH key");
            A("dlgName",
              "Имя файла (без .pub)",
              "File name (without .pub)");
            A("dlgComment",
              "Комментарий",
              "Comment");
            A("dlgPass",
              "Пароль ключа (можно пусто)",
              "Key passphrase (may be empty)");
            A("dlgOk",
              "Создать",
              "Create");
            A("dlgCancel",
              "Отмена",
              "Cancel");
            A("badName",
              "Недопустимое имя: {0}",
              "Invalid name: {0}");
            A("overwrite",
              "Файл {0} уже есть. Перезаписать?",
              "File {0} already exists. Overwrite?");
            A("titleNewKey",
              "Новый ключ",
              "New key");
            A("creating",
              "Создаю ключ {0} ...",
              "Creating key {0} ...");
            A("keygenFail",
              "ssh-keygen не смог создать ключ",
              "ssh-keygen failed to create the key");
            A("keygenFailTimeout",
              "ssh-keygen не смог создать ключ (таймаут)",
              "ssh-keygen failed to create the key (timeout)");
            A("writeFail",
              "Не удалось записать {0}: {1}",
              "Failed to write {0}: {1}");
            A("rollbackFail",
              "Откат не удался. Старый ключ: {0} / {1}, новый: {2} / {3}",
              "Rollback failed. Old key: {0} / {1}, new: {2} / {3}");
            A("oldRestored",
              "Старый ключ {0} восстановлен.",
              "Old key {0} restored.");
            A("bakManual",
              "Резервную копию старого ключа удалите вручную: {0} / {1}",
              "Delete the backup of the old key manually: {0} / {1}");
            A("created",
              "Создан {0}",
              "Created {0}");
            A("createFail",
              "Не удалось создать ключ {0}: {1}",
              "Failed to create key {0}: {1}");
            A("bakKept",
              "Резервная копия старого ключа оставлена: {0}",
              "Backup of the old key kept: {0}");
            A("titleDelete",
              "Удаление",
              "Deletion");
            A("pickKey",
              "Выберите ключ в списке.",
              "Select a key in the list.");
            A("delConfirm",
              "Удалить ключ {0} с этого ПК?",
              "Delete key {0} from this PC?");
            A("delOk",
              "Удалён локально: {0}",
              "Deleted locally: {0}");
            A("delPartial",
              "Ключ {0} удалён не полностью",
              "Key {0} was not fully deleted");
            A("delAllConfirm",
              "Удалить все ключи из списка ({0} шт.) в {1} ?\n\nДругие файлы папки (config, known_hosts, authorized_keys) не затрагиваются.",
              "Delete all keys in the list ({0}) in {1} ?\n\nOther files in the folder (config, known_hosts, authorized_keys) are not affected.");
            A("clipFail",
              "Не удалось записать в буфер обмена: {0}",
              "Failed to write to the clipboard: {0}");
            A("titleCopy",
              "Копирование",
              "Copying");
            A("pickKeyShort",
              "Выберите ключ.",
              "Select a key.");
            A("pubCopied",
              "Публичный ключ скопирован: {0}",
              "Public key copied: {0}");
            A("badIpSkipped",
              "Неверный адрес сервера, пропущен: {0}",
              "Invalid server address, skipped: {0}");
            A("pickIp",
              "Выберите IP в списке (Ctrl — несколько).",
              "Select an IP in the list (Ctrl for several).");
            A("titleServers",
              "Серверы",
              "Servers");
            A("pickKeyTable",
              "Выберите ключ в таблице.",
              "Select a key in the table.");
            A("noPriv",
              "Нет приватного ключа: {0}",
              "No private key: {0}");
            A("noSsh",
              "Не найден ssh. Установите OpenSSH Client.",
              "ssh not found. Install OpenSSH Client.");
            A("badUser",
              "Недопустимый логин: {0}\nРазрешены латинские буквы, цифры, _ . - (до 32 символов, не с цифры),\nа также формы DOMAIN\\user, user@domain и завершающий $.",
              "Invalid login: {0}\nAllowed: Latin letters, digits, _ . - (up to 32 characters, not starting with a digit),\nas well as DOMAIN\\user, user@domain and a trailing $.");
            A("pickServer",
              "Выберите сервер в списке (Ctrl — несколько) или укажите IP в поле.",
              "Select a server in the list (Ctrl for several) or enter an IP in the field.");
            A("badHost",
              "Недопустимый адрес сервера:\n{0}",
              "Invalid server address:\n{0}");
            A("unsafePath",
              "Путь к ключу содержит символы $ % ` \" ' и не может быть безопасно вставлен в команду:\n{0}",
              "The key path contains the characters $ % ` \" ' and cannot be safely pasted into a command:\n{0}");
            A("loginCmdCopied",
              "Команды входа ключом {0} скопированы в буфер:",
              "Login commands for key {0} copied to the clipboard:");
            A("pubReadFail",
              "Не удалось прочитать публичный ключ: {0}",
              "Failed to read the public key: {0}");
            A("pubBadFormat",
              "Неизвестный формат публичного ключа: {0}",
              "Unknown public key format: {0}");
            A("adding",
              "Добавляю ключ: {0}",
              "Adding key: {0}");
            A("oneSession",
              "Одна SSH-сессия на сервер. Если ключа ещё нет — один раз спросит пароль.",
              "One SSH session per server. If the key is not there yet, you will be asked for the password once.");
            A("sepUpload",
              "---------- залить {0} на {1} ----------",
              "---------- upload {0} to {1} ----------");
            A("uploadFail",
              "{0} — не удалось залить ключ {1}",
              "{0} — failed to upload key {1}");
            A("uploadSummary",
              "Итог заливки {0}: {1} OK / {2} FAIL из {3}",
              "Upload result for {0}: {1} OK / {2} FAIL of {3}");
            A("checking",
              "Проверяю ключ: {0}",
              "Checking key: {0}");
            A("sepCheck",
              "---------- проверка {0} на {1} ----------",
              "---------- checking {0} on {1} ----------");
            A("checkSummary",
              "Итог проверки {0}: {1} OK / {2} FAIL из {3}",
              "Check result for {0}: {1} OK / {2} FAIL of {3}");
            A("revokeConfirm",
              "Убрать ключ \"{0}\" с серверов:\n{1}\n\n{2}",
              "Remove key \"{0}\" from the servers:\n{1}\n\n{2}");
            A("removing",
              "Убираю ключ: {0}",
              "Removing key: {0}");
            A("sepRevoke",
              "---------- убрать {0} с {1} ----------",
              "---------- removing {0} from {1} ----------");
            A("revokeFail",
              "{0} — не удалось убрать ключ {1}",
              "{0} — failed to remove key {1}");
            A("revokedNoPriv",
              "{0} — ключ {1} убран из authorized_keys; вход не проверен (нет приватного ключа)",
              "{0} — key {1} removed from authorized_keys; login not verified (no private key)");
            A("stillWorks",
              "{0} — ключ {1} мог уйти из файла, но вход по нему всё ещё работает",
              "{0} — key {1} may be gone from the file, but login with it still works");
            A("stillAccepted",
              "{0} — ключ {1} ещё принимается сервером",
              "{0} — key {1} is still accepted by the server");
            A("revokedUnknown",
              "{0} — ключ {1} убран из authorized_keys; вход не проверен ({2})",
              "{0} — key {1} removed from authorized_keys; login not verified ({2})");
            A("revokedOk",
              "{0} — ключ {1} убран, вход по нему закрыт",
              "{0} — key {1} removed, login with it is closed");
            A("revokeSummary",
              "Итог удаления {0}: {1} OK / {2} FAIL из {3}",
              "Removal result for {0}: {1} OK / {2} FAIL of {3}");
            A("reasonOtherKey",
              "вход выполнен другим ключом из config",
              "login succeeded with a different key from config");
            A("reasonTimeout",
              "таймаут",
              "timeout");
            A("reasonExit",
              "ssh завершился с кодом {0}",
              "ssh exited with code {0}");
            A("checkingLogin",
              "Проверяю вход ключом {0} без пароля: {1} ...",
              "Checking passwordless login with key {0}: {1} ...");
            A("loginOk",
              "{0} — вход ключом {1} без пароля работает",
              "{0} — passwordless login with key {1} works");
            A("loginNeedsPass",
              "{0} — ключ {1} на сервере, нужен пароль ключа",
              "{0} — key {1} is on the server, key passphrase required");
            A("loginDenied",
              "{0} — вход ключом {1} не работает",
              "{0} — login with key {1} does not work");
            A("loginUnknown",
              "{0} — вход ключом {1} проверить не удалось ({2})",
              "{0} — could not verify login with key {1} ({2})");
            A("deleteFail",
              "Не удалось удалить {0}: {1}",
              "Failed to delete {0}: {1}");
        }
    }

    sealed class SshKeyInfo
    {
        public string Name;
        public string PubPath;
        public string PrivPath;
        public bool HasPriv;
        public string Finger;
    }

    sealed class ProcResult
    {
        public int ExitCode = -1;
        public bool TimedOut;
        public string StdOut = "";
        public string StdErr = "";
    }

    enum LoginCheck { Works, AcceptedNeedsPassphrase, Denied, Unknown }

    sealed class MainForm : Form
    {
        const int KeygenTimeoutMs = 60000;
        const int FingerTimeoutMs = 10000;
        const int CheckTimeoutMs = 45000;

        static readonly string[] ReservedNames =
        {
            "config", "known_hosts", "known_hosts.old", "known_hosts2",
            "authorized_keys", "authorized_keys2", "environment", "rc"
        };
        static readonly string[] KnownKeyTypes =
        {
            "ssh-ed25519", "ssh-rsa", "ssh-dss",
            "ecdsa-sha2-nistp256", "ecdsa-sha2-nistp384", "ecdsa-sha2-nistp521",
            "sk-ssh-ed25519@openssh.com", "sk-ecdsa-sha2-nistp256@openssh.com"
        };
        static readonly Regex UserRx = new Regex(@"^(?:[A-Za-z0-9][A-Za-z0-9.-]{0,62}\\)?[A-Za-z_][A-Za-z0-9_.-]{0,31}\$?(?:@[A-Za-z0-9][A-Za-z0-9.-]{0,252})?\z");
        static readonly Regex HostRx = new Regex(@"^[A-Za-z0-9][A-Za-z0-9.\-]*\z");
        static readonly Regex KeyNameRx = new Regex(@"^[A-Za-z0-9][A-Za-z0-9._-]*\z");
        static readonly Regex DeviceNameRx = new Regex(@"^(con|prn|aux|nul|com[1-9]|lpt[1-9])(\..*)?\z", RegexOptions.IgnoreCase);
        static readonly Regex BlobRx = new Regex(@"^[A-Za-z0-9+/]{60,}={0,2}\z");
        const string TempKeyPrefix = ".skm-new-";
        const string BackupKeyPrefix = ".skm-bak-";

        const string AwkFindBlob = "{l=$0;sub(/\\r$/,\"\",l);n=split(l,a);for(i=1;i<n;i++)if(a[i]~/^(ssh-|ecdsa-|sk-)/&&a[i+1]==b)f=1}END{exit (f?0:1)}";
        const string AwkDropBlob = "{l=$0;sub(/\\r$/,\"\",l);n=split(l,a);k=1;for(i=1;i<n;i++)if(a[i]~/^(ssh-|ecdsa-|sk-)/&&a[i+1]==b)k=0}k";

        readonly int uiThreadId = Thread.CurrentThread.ManagedThreadId;
        int refreshVersion;

        readonly string sshDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
        readonly string settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SshKeyManager", "settings.ini");
        readonly string sshExe = FindOpenSsh("ssh");
        readonly string keygenExe = FindOpenSsh("ssh-keygen");

        List<SshKeyInfo> keys = new List<SshKeyInfo>();
        readonly List<Control> busyControls = new List<Control>();
        bool busy;
        Label lblKeys;
        DataGridView grid;
        TextBox txtPub;
        TextBox txtUser;
        NumericUpDown numPort;
        TextBox txtIp;
        ListBox listIps;
        RichTextBox logBox;
        ComboBox cmbLang;
        bool langSync;
        readonly List<KeyValuePair<Control, string>> bound = new List<KeyValuePair<Control, string>>();

        public MainForm()
        {
            Text = "SSH Key Manager";
            Font = new Font("Segoe UI", 9f);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(760, 740);
            Size = new Size(800, 780);
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { }

            lblKeys = new Label
            {
                Location = new Point(16, 12),
                Size = new Size(582, 20),
                AutoEllipsis = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var btnRefresh = new Button { Location = new Point(670, 8), Width = 96, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            cmbLang = new ComboBox
            {
                Location = new Point(606, 9),
                Width = 56,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            cmbLang.Items.AddRange(new object[] { "RU", "EN" });
            cmbLang.SelectedIndexChanged += delegate { OnLangChanged(); };
            btnRefresh.Click += delegate { RefreshKeys(); };

            grid = new DataGridView
            {
                Location = new Point(16, 36),
                Size = new Size(750, 150),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            grid.Columns.Add("Name", L.T("colName"));
            grid.Columns.Add("Priv", L.T("colPriv"));
            grid.Columns.Add("Finger", L.T("colFinger"));
            grid.Columns[0].FillWeight = 20;
            grid.Columns[1].FillWeight = 12;
            grid.Columns[2].FillWeight = 68;
            grid.SelectionChanged += delegate { ShowSelectedPub(); };

            txtPub = new TextBox
            {
                Location = new Point(16, 192),
                Size = new Size(750, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                Multiline = true,
                Font = new Font("Consolas", 8.5f)
            };

            var btnNew = new Button { Location = new Point(16, 240), Width = 120 };
            btnNew.Click += delegate { RunSafe(CreateKey); };

            var btnDel = new Button { Location = new Point(144, 240), Width = 90 };
            btnDel.Click += delegate { DeleteSelected(); };

            var btnDelAll = new Button { Location = new Point(242, 240), Width = 100 };
            btnDelAll.Click += delegate { DeleteAll(); };

            var btnCopy = new Button { Location = new Point(350, 240), Width = 120 };
            btnCopy.Click += delegate { CopyPub(); };

            var grp = new GroupBox
            {
                Location = new Point(16, 278),
                Size = new Size(750, 228),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lUser = new Label { Location = new Point(12, 28), AutoSize = true };
            txtUser = new TextBox { Text = "root", Location = new Point(60, 24), Width = 120 };

            var lPort = new Label { Location = new Point(196, 28), AutoSize = true };
            numPort = new NumericUpDown
            {
                Location = new Point(236, 24),
                Width = 70,
                Minimum = 1,
                Maximum = 65535,
                Value = 22
            };

            var lIp = new Label { Location = new Point(12, 58), AutoSize = true };
            txtIp = new TextBox { Location = new Point(12, 78), Width = 520, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            txtIp.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    AddIpFromBox();
                }
            };

            var btnAddIp = new Button { Location = new Point(544, 76), Width = 90, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnAddIp.Click += delegate { AddIpFromBox(); };

            var btnRemoveIp = new Button { Location = new Point(644, 76), Width = 90, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnRemoveIp.Click += delegate { RemoveSelectedIp(); };

            var lList = new Label { Location = new Point(12, 108), AutoSize = true };
            listIps = new ListBox
            {
                Location = new Point(12, 128),
                Size = new Size(722, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                SelectionMode = SelectionMode.MultiExtended
            };

            var btnUp = new Button { Location = new Point(12, 186), Width = 150 };
            btnUp.Click += delegate { RunSafe(Upload); };

            var btnCheck = new Button { Location = new Point(172, 186), Width = 140 };
            btnCheck.Click += delegate { RunSafe(CheckOnly); };

            var btnRevoke = new Button { Location = new Point(322, 186), Width = 140 };
            btnRevoke.Click += delegate { RunSafe(Revoke); };

            var btnCmd = new Button { Location = new Point(472, 186), Width = 130 };
            btnCmd.Click += delegate { CopyLoginCommand(); };

            grp.Controls.AddRange(new Control[] { lUser, txtUser, lPort, numPort, lIp, txtIp, btnAddIp, btnRemoveIp, lList, listIps, btnUp, btnCheck, btnRevoke, btnCmd });

            busyControls.AddRange(new Control[] { cmbLang, btnRefresh, btnNew, btnDel, btnDelAll, btnCopy, btnAddIp, btnRemoveIp, btnUp, btnCheck, btnRevoke, btnCmd });

            logBox = new RichTextBox
            {
                Location = new Point(16, 518),
                Size = new Size(750, 206),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Consolas", 9f),
                HideSelection = false
            };

            Bind(btnRefresh, "btnRefresh");
            Bind(btnNew, "btnNew");
            Bind(btnDel, "btnDel");
            Bind(btnDelAll, "btnDelAll");
            Bind(btnCopy, "btnCopy");
            Bind(grp, "grpServer");
            Bind(lUser, "lblUser");
            Bind(lPort, "lblPort");
            Bind(lIp, "lblIp");
            Bind(btnAddIp, "btnAddIp");
            Bind(btnRemoveIp, "btnRemoveIp");
            Bind(lList, "lblList");
            Bind(btnUp, "btnUp");
            Bind(btnCheck, "btnCheck");
            Bind(btnRevoke, "btnRevoke");
            Bind(btnCmd, "btnCmd");

            Controls.AddRange(new Control[] { lblKeys, cmbLang, btnRefresh, grid, txtPub, btnNew, btnDel, btnDelAll, btnCopy, grp, logBox });
            FormClosing += delegate(object s, FormClosingEventArgs e)
            {
                if (busy)
                {
                    e.Cancel = true;
                    MessageBox.Show(this, L.T("busyWait"), "SSH Key Manager");
                    return;
                }
                SaveSettings();
            };

            LoadSettings();
            langSync = true;
            cmbLang.SelectedIndex = L.Lang == "ru" ? 0 : 1;
            langSync = false;
            ApplyTexts();

            if (string.IsNullOrEmpty(keygenExe) || string.IsNullOrEmpty(sshExe))
                Log(L.T("noOpenSsh"), Color.Red);

            Load += delegate { RefreshKeys(); };
        }

        void Bind(Control c, string key)
        {
            bound.Add(new KeyValuePair<Control, string>(c, key));
        }

        void ApplyTexts()
        {
            foreach (var b in bound)
                b.Key.Text = L.T(b.Value);
            grid.Columns[0].HeaderText = L.T("colName");
            grid.Columns[1].HeaderText = L.T("colPriv");
            grid.Columns[2].HeaderText = L.T("colFinger");
            foreach (DataGridViewRow row in grid.Rows)
            {
                var k = row.Tag as SshKeyInfo;
                if (k != null) row.Cells[1].Value = L.T(k.HasPriv ? "yes" : "no");
            }
            lblKeys.Text = L.F("keysLabel", sshDir);
        }

        void OnLangChanged()
        {
            if (langSync) return;
            var lang = cmbLang.SelectedIndex == 0 ? "ru" : "en";
            if (busy)
            {
                langSync = true;
                cmbLang.SelectedIndex = L.Lang == "ru" ? 0 : 1;
                langSync = false;
                return;
            }
            L.Lang = lang;
            ApplyTexts();
        }

        static bool IsFileError(Exception ex)
        {
            return ex is IOException || ex is UnauthorizedAccessException ||
                   ex is ArgumentException || ex is NotSupportedException ||
                   ex is System.Security.SecurityException;
        }

        void OnUi(Action action)
        {
            try
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(action);
            }
            catch (InvalidOperationException) { }
        }

        async void RunSafe(Func<Action> prepare)
        {
            if (busy) return;
            Action work;
            try { work = prepare(); }
            catch (Exception ex)
            {
                Log("[ERROR] " + ex.Message, Color.Red);
                return;
            }
            if (work == null) return;

            SetBusy(true);
            try { await Task.Run(work); }
            catch (Exception ex) { Log("[ERROR] " + ex.Message, Color.Red); }
            finally { SetBusy(false); }
        }

        void SetBusy(bool value)
        {
            busy = value;
            foreach (var c in busyControls)
                c.Enabled = !value;
            UseWaitCursor = value;
        }

        static string FindOpenSsh(string name)
        {
            var file = name + ".exe";
            var win = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            if (!string.IsNullOrEmpty(win))
            {
                var sys = Path.Combine(win, "System32", "OpenSSH", file);
                if (File.Exists(sys)) return sys;
                if (!Environment.Is64BitProcess)
                {
                    var native = Path.Combine(win, "Sysnative", "OpenSSH", file);
                    if (File.Exists(native)) return native;
                }
            }
            var path = Environment.GetEnvironmentVariable("PATH") ?? "";
            foreach (var raw in path.Split(Path.PathSeparator))
            {
                var dir = raw.Trim().Trim('"');
                if (dir.Length == 0) continue;
                try
                {
                    if (!IsAbsoluteDir(dir)) continue;
                    var exe = Path.Combine(dir, file);
                    if (File.Exists(exe)) return exe;
                }
                catch (ArgumentException) { }
            }
            return null;
        }

        static bool IsAbsoluteDir(string dir)
        {
            if (!Path.IsPathRooted(dir)) return false;
            if (dir.StartsWith(@"\\")) return true;
            return dir.Length >= 3 && dir[1] == ':' && (dir[2] == '\\' || dir[2] == '/');
        }

        void Log(string text, Color color)
        {
            var line = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text + "\r\n";
            try
            {
                if (logBox.IsDisposed) return;
                if (Thread.CurrentThread.ManagedThreadId != uiThreadId)
                {
                    if (!logBox.IsHandleCreated) return;
                    logBox.BeginInvoke(new Action<string, Color>(AppendLog), line, color);
                    return;
                }
            }
            catch (InvalidOperationException) { return; }
            AppendLog(line, color);
        }

        void AppendLog(string line, Color color)
        {
            if (logBox.IsDisposed) return;
            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionLength = 0;
            logBox.SelectionColor = color;
            logBox.AppendText(line);
            logBox.SelectionColor = logBox.ForeColor;
            logBox.ScrollToCaret();
        }

        void LogStdErr(string stderr)
        {
            if (string.IsNullOrEmpty(stderr)) return;
            foreach (var l in stderr.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var t = l.Trim();
                if (t.Length == 0 ||
                    t.StartsWith("debug", StringComparison.Ordinal) ||
                    t.StartsWith("OpenSSH_", StringComparison.Ordinal) ||
                    t.StartsWith("Authenticated to", StringComparison.Ordinal) ||
                    t.StartsWith("Transferred:", StringComparison.Ordinal) ||
                    t.StartsWith("Bytes per second:", StringComparison.Ordinal))
                    continue;
                Log("  " + l.TrimEnd(), Color.DimGray);
            }
        }

        void LoadSettings()
        {
            try
            {
                if (!File.Exists(settingsPath)) return;
                foreach (var raw in File.ReadAllLines(settingsPath))
                {
                    var line = raw.Trim();
                    var eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    var key = line.Substring(0, eq);
                    var val = line.Substring(eq + 1);
                    if (key == "User" && val.Length > 0) txtUser.Text = val;
                    else if (key == "Port")
                    {
                        int p;
                        if (int.TryParse(val, out p) && p >= 1 && p <= 65535) numPort.Value = p;
                    }
                    else if (key == "Lang")
                    {
                        if (val == "ru" || val == "en") L.Lang = val;
                    }
                    else if (key == "Ips")
                    {
                        foreach (var ip in ParseIps(val))
                            if (IsValidHost(ip) && !ListHasIp(ip)) listIps.Items.Add(ip);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(L.F("settingsReadFail", settingsPath, ex.Message), Color.DarkOrange);
            }
        }

        void SaveSettings()
        {
            try
            {
                var dir = Path.GetDirectoryName(settingsPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.WriteAllLines(settingsPath, new[]
                {
                    "User=" + txtUser.Text.Trim(),
                    "Port=" + ((int)numPort.Value),
                    "Ips=" + string.Join(",", GetAllListIps()),
                    "Lang=" + L.Lang
                });
            }
            catch { }
        }

        static bool IsReservedName(string name)
        {
            return ReservedNames.Any(r => string.Equals(r, name, StringComparison.OrdinalIgnoreCase));
        }

        static bool IsSafeKeyName(string name)
        {
            return !string.IsNullOrEmpty(name) &&
                   KeyNameRx.IsMatch(name) &&
                   !name.EndsWith(".", StringComparison.Ordinal) &&
                   !name.EndsWith(".pub", StringComparison.OrdinalIgnoreCase) &&
                   !IsReservedName(name) &&
                   !DeviceNameRx.IsMatch(name);
        }

        List<SshKeyInfo> LoadKeys()
        {
            var list = new List<SshKeyInfo>();
            if (!Directory.Exists(sshDir)) return list;
            foreach (var pub in Directory.GetFiles(sshDir, "*.pub"))
            {
                if (!pub.EndsWith(".pub", StringComparison.OrdinalIgnoreCase))
                    continue;
                var name = Path.GetFileNameWithoutExtension(pub);
                if (!IsSafeKeyName(name))
                    continue;
                var priv = Path.Combine(sshDir, name);
                var finger = "";
                if (!string.IsNullOrEmpty(keygenExe))
                {
                    try
                    {
                        var r = RunCaptured(keygenExe, "-lf " + QuoteArg(pub), FingerTimeoutMs);
                        if (r.ExitCode == 0) finger = r.StdOut.Trim();
                    }
                    catch (Exception ex)
                    {
                        Log(L.F("fingerFail", pub, ex.Message), Color.DarkOrange);
                    }
                }
                list.Add(new SshKeyInfo
                {
                    Name = name,
                    PubPath = pub,
                    PrivPath = priv,
                    HasPriv = File.Exists(priv),
                    Finger = finger
                });
            }
            return list;
        }

        void RefreshKeys()
        {
            RefreshKeys(null);
        }

        async void RefreshKeys(string selectName)
        {
            var prev = SelectedKey();
            if (selectName == null && prev != null) selectName = prev.Name;
            var version = ++refreshVersion;

            List<SshKeyInfo> loaded;
            try { loaded = await Task.Run(new Func<List<SshKeyInfo>>(LoadKeys)); }
            catch (Exception ex)
            {
                if (!IsFileError(ex)) throw;
                loaded = new List<SshKeyInfo>();
                Log(L.F("readDirFail", sshDir, ex.Message), Color.Red);
            }
            if (version != refreshVersion || IsDisposed) return;
            keys = loaded;

            grid.Rows.Clear();
            foreach (var k in keys)
            {
                var row = new DataGridViewRow();
                row.CreateCells(grid, k.Name, L.T(k.HasPriv ? "yes" : "no"), k.Finger);
                row.Tag = k;
                grid.Rows.Add(row);
            }
            if (grid.Rows.Count > 0)
            {
                var target = FindRow(selectName) ?? FindRow("id_ed25519") ?? FindRow("id_rsa") ?? grid.Rows[0];
                grid.ClearSelection();
                target.Selected = true;
                grid.CurrentCell = target.Cells[0];
            }
            lblKeys.Text = L.F("keysLabel", sshDir);
            ShowSelectedPub();
        }

        DataGridViewRow FindRow(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            foreach (DataGridViewRow row in grid.Rows)
            {
                var k = row.Tag as SshKeyInfo;
                if (k != null && string.Equals(k.Name, name, StringComparison.OrdinalIgnoreCase))
                    return row;
            }
            return null;
        }

        SshKeyInfo SelectedKey()
        {
            if (grid.CurrentRow == null) return null;
            return grid.CurrentRow.Tag as SshKeyInfo;
        }

        static string ReadPubLine(SshKeyInfo k)
        {
            if (k == null) return "";
            try
            {
                if (!File.Exists(k.PubPath)) return "";
                return (File.ReadAllLines(k.PubPath).FirstOrDefault(s => s.Trim().Length > 0) ?? "").Trim();
            }
            catch (Exception ex)
            {
                if (!IsFileError(ex)) throw;
                return "";
            }
        }

        static string PubBlob(string pubLine)
        {
            var parts = (pubLine ?? "").Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return null;
            if (!KnownKeyTypes.Contains(parts[0])) return null;
            return BlobRx.IsMatch(parts[1]) ? parts[1] : null;
        }

        static string KeyLabel(SshKeyInfo k)
        {
            if (k == null) return "?";
            var finger = (k.Finger ?? "").Trim();
            return finger.Length == 0 ? k.Name : k.Name + " — " + finger;
        }

        void ShowSelectedPub()
        {
            txtPub.Text = ReadPubLine(SelectedKey());
        }

        static string ValidateKeyName(string name)
        {
            if (!KeyNameRx.IsMatch(name))
                return L.T("nameChars");
            if (name.EndsWith(".", StringComparison.Ordinal))
                return L.T("nameDot");
            if (name.EndsWith(".pub", StringComparison.OrdinalIgnoreCase))
                return L.T("namePub");
            if (IsReservedName(name))
                return L.F("nameReserved", name);
            if (DeviceNameRx.IsMatch(name))
                return L.F("nameDevice", name);
            return null;
        }

        Action CreateKey()
        {
            if (string.IsNullOrEmpty(keygenExe))
            {
                MessageBox.Show(this, L.T("noKeygen"), L.T("titleError"));
                return null;
            }

            using (var f = new Form())
            {
                f.Text = L.T("dlgTitle");
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.ClientSize = new Size(420, 220);
                f.MaximizeBox = false;
                f.MinimizeBox = false;

                var tName = new TextBox { Text = "id_ed25519", Location = new Point(16, 36), Width = 388 };
                var tComment = new TextBox { Text = Environment.UserName + "@" + Environment.MachineName, Location = new Point(16, 90), Width = 388 };
                var tPass = new TextBox { UseSystemPasswordChar = true, Location = new Point(16, 144), Width = 388 };
                var ok = new Button { Text = L.T("dlgOk"), Location = new Point(216, 180), DialogResult = DialogResult.OK };
                var cancel = new Button { Text = L.T("dlgCancel"), Location = new Point(312, 180), DialogResult = DialogResult.Cancel };

                f.Controls.Add(new Label { Text = L.T("dlgName"), Location = new Point(16, 16), AutoSize = true });
                f.Controls.Add(tName);
                f.Controls.Add(new Label { Text = L.T("dlgComment"), Location = new Point(16, 70), AutoSize = true });
                f.Controls.Add(tComment);
                f.Controls.Add(new Label { Text = L.T("dlgPass"), Location = new Point(16, 124), AutoSize = true });
                f.Controls.Add(tPass);
                f.Controls.Add(ok);
                f.Controls.Add(cancel);
                f.AcceptButton = ok;
                f.CancelButton = cancel;

                string name = null;
                ok.Click += delegate
                {
                    var n = tName.Text.Trim();
                    var err = ValidateKeyName(n);
                    if (err != null)
                    {
                        f.DialogResult = DialogResult.None;
                        MessageBox.Show(f, err, L.T("titleError"));
                        tName.Focus();
                        return;
                    }
                    string d;
                    try { d = Path.Combine(sshDir, n); }
                    catch (ArgumentException)
                    {
                        f.DialogResult = DialogResult.None;
                        MessageBox.Show(f, L.F("badName", n), L.T("titleError"));
                        return;
                    }
                    if (File.Exists(d) || File.Exists(d + ".pub"))
                    {
                        if (MessageBox.Show(f, L.F("overwrite", n), L.T("titleNewKey"),
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        {
                            f.DialogResult = DialogResult.None;
                            return;
                        }
                    }
                    name = n;
                };

                if (f.ShowDialog(this) != DialogResult.OK || name == null) return null;

                var comment = string.IsNullOrWhiteSpace(tComment.Text)
                    ? Environment.UserName + "@" + Environment.MachineName
                    : tComment.Text;
                var pass = tPass.Text;
                return delegate
                {
                    if (GenerateKey(name, comment, pass))
                        OnUi(delegate { RefreshKeys(name); });
                };
            }
        }

        bool GenerateKey(string name, string comment, string pass)
        {
            var dest = Path.Combine(sshDir, name);
            var destPub = dest + ".pub";
            var id = Guid.NewGuid().ToString("N");
            var tmp = Path.Combine(sshDir, TempKeyPrefix + id);
            var tmpPub = tmp + ".pub";
            var bak = Path.Combine(sshDir, BackupKeyPrefix + id);
            var bakPub = bak + ".pub";
            var privMoved = false;
            var keepFiles = false;

            Log(L.F("creating", name), Color.DarkBlue);
            try
            {
                Directory.CreateDirectory(sshDir);
                var args = "-t ed25519 -f " + QuoteArg(tmp) + " -C " + QuoteArg(comment) + " -N " + QuoteArg(pass);
                var r = RunCaptured(keygenExe, args, KeygenTimeoutMs);
                if (r.TimedOut || r.ExitCode != 0 || !File.Exists(tmp) || !File.Exists(tmpPub))
                {
                    LogStdErr(r.StdErr);
                    Log("[FAIL] " + L.T(r.TimedOut ? "keygenFailTimeout" : "keygenFail"), Color.Red);
                    return false;
                }

                try
                {
                    if (File.Exists(dest)) File.Move(dest, bak);
                    File.Move(tmp, dest);
                }
                catch
                {
                    if (!File.Exists(tmp) && File.Exists(dest))
                    {
                        if (!RollbackPair(dest, bak, destPub, bakPub)) keepFiles = true;
                    }
                    else if (!RestoreIfMissing(dest, bak)) keepFiles = true;
                    throw;
                }
                privMoved = true;

                try
                {
                    if (File.Exists(destPub)) File.Replace(tmpPub, destPub, bakPub);
                    else File.Move(tmpPub, destPub);
                }
                catch (Exception ex)
                {
                    Log("[FAIL] " + L.F("writeFail", destPub, ex.Message), Color.Red);
                    if (!RollbackPair(dest, bak, destPub, bakPub))
                    {
                        keepFiles = true;
                        Log("[FAIL] " + L.F("rollbackFail", bak, bakPub, dest, tmpPub), Color.Red);
                    }
                    else
                    {
                        Log(L.F("oldRestored", name), Color.DarkOrange);
                    }
                    if (!IsFileError(ex)) throw;
                    return false;
                }

                if (!TryDelete(bak) || !TryDelete(bakPub))
                    Log(L.F("bakManual", bak, bakPub), Color.DarkOrange);
                Log("[OK] " + L.F("created", dest), Color.DarkGreen);
                return true;
            }
            catch (Exception ex)
            {
                if (!IsFileError(ex) && !(ex is Win32Exception)) throw;
                Log("[FAIL] " + L.F("createFail", name, ex.Message), Color.Red);
                if (keepFiles)
                    Log("[FAIL] " + L.F("bakKept", bak), Color.Red);
                return false;
            }
            finally
            {
                if (!keepFiles)
                {
                    if (!privMoved) TryDeleteQuiet(tmp);
                    TryDeleteQuiet(tmpPub);
                }
            }
        }

        static bool RestoreIfMissing(string target, string backup)
        {
            try
            {
                if (!File.Exists(target) && File.Exists(backup)) File.Move(backup, target);
                return true;
            }
            catch (Exception ex)
            {
                if (!IsFileError(ex)) throw;
                return false;
            }
        }

        static bool RollbackPair(string dest, string bak, string destPub, string bakPub)
        {
            try
            {
                if (File.Exists(dest)) File.Delete(dest);
                if (File.Exists(bak)) File.Move(bak, dest);
                if (!File.Exists(destPub) && File.Exists(bakPub)) File.Move(bakPub, destPub);
                return true;
            }
            catch (Exception ex)
            {
                if (!IsFileError(ex)) throw;
                return false;
            }
        }

        void DeleteSelected()
        {
            var k = SelectedKey();
            if (k == null)
            {
                MessageBox.Show(this, L.T("pickKey"), L.T("titleDelete"));
                return;
            }
            if (MessageBox.Show(this, L.F("delConfirm", k.Name), L.T("titleDelete"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;
            var okPriv = TryDelete(k.PrivPath);
            var okPub = TryDelete(k.PubPath);
            if (okPriv && okPub)
                Log("[OK] " + L.F("delOk", k.Name), Color.DarkGreen);
            else
                Log("[FAIL] " + L.F("delPartial", k.Name), Color.Red);
            RefreshKeys();
        }

        void DeleteAll()
        {
            if (keys.Count == 0) return;
            if (MessageBox.Show(this, L.F("delAllConfirm", keys.Count, sshDir), L.T("titleDelete"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;
            foreach (var k in keys)
            {
                var okPriv = TryDelete(k.PrivPath);
                var okPub = TryDelete(k.PubPath);
                if (okPriv && okPub)
                    Log("[OK] " + L.F("delOk", k.Name), Color.DarkGreen);
                else
                    Log("[FAIL] " + L.F("delPartial", k.Name), Color.Red);
            }
            RefreshKeys();
        }

        bool TrySetClipboard(string text)
        {
            try
            {
                Clipboard.SetText(text);
                return true;
            }
            catch (ExternalException ex)
            {
                Log("[FAIL] " + L.F("clipFail", ex.Message), Color.Red);
                return false;
            }
        }

        void CopyPub()
        {
            var line = ReadPubLine(SelectedKey());
            if (string.IsNullOrEmpty(line))
            {
                MessageBox.Show(this, L.T("pickKeyShort"), L.T("titleCopy"));
                return;
            }
            if (!TrySetClipboard(line)) return;
            Log(L.F("pubCopied", KeyLabel(SelectedKey())), Color.DarkGreen);
        }

        static string[] ParseIps(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new string[0];
            return Regex.Split(raw, @"[\s,;]+")
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToArray();
        }

        static bool IsValidUser(string user)
        {
            return !string.IsNullOrEmpty(user) && UserRx.IsMatch(user);
        }

        static bool IsValidHost(string host)
        {
            if (string.IsNullOrEmpty(host) || host.Length > 253) return false;
            if (host.IndexOf('%') >= 0) return false;
            IPAddress addr;
            return IPAddress.TryParse(host, out addr) || HostRx.IsMatch(host);
        }

        bool ListHasIp(string ip)
        {
            foreach (var item in listIps.Items)
                if (string.Equals(Convert.ToString(item), ip, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        void AddIpFromBox()
        {
            var added = new List<string>();
            foreach (var ip in ParseIps(txtIp.Text))
            {
                if (!IsValidHost(ip))
                {
                    Log(L.F("badIpSkipped", ip), Color.Red);
                    continue;
                }
                if (!ListHasIp(ip)) listIps.Items.Add(ip);
                if (!added.Exists(x => string.Equals(x, ip, StringComparison.OrdinalIgnoreCase)))
                    added.Add(ip);
            }
            if (added.Count > 0)
            {
                listIps.ClearSelected();
                for (var i = 0; i < listIps.Items.Count; i++)
                {
                    var ip = Convert.ToString(listIps.Items[i]);
                    if (added.Exists(x => string.Equals(x, ip, StringComparison.OrdinalIgnoreCase)))
                        listIps.SetSelected(i, true);
                }
            }
            txtIp.Clear();
            listIps.Focus();
        }

        void RemoveSelectedIp()
        {
            if (listIps.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, L.T("pickIp"), L.T("grpServer"));
                return;
            }
            var toRemove = new List<object>();
            foreach (var item in listIps.SelectedItems)
                toRemove.Add(item);
            foreach (var item in toRemove)
                listIps.Items.Remove(item);
        }

        string[] GetAllListIps()
        {
            var set = new List<string>();
            foreach (var item in listIps.Items)
            {
                var ip = Convert.ToString(item);
                if (!string.IsNullOrEmpty(ip) &&
                    !set.Exists(x => string.Equals(x, ip, StringComparison.OrdinalIgnoreCase)))
                    set.Add(ip);
            }
            return set.ToArray();
        }

        string[] GetTargetIps()
        {
            var set = new List<string>();
            foreach (var ip in ParseIps(txtIp.Text))
                if (!set.Exists(x => string.Equals(x, ip, StringComparison.OrdinalIgnoreCase)))
                    set.Add(ip);
            foreach (var item in listIps.SelectedItems)
            {
                var ip = Convert.ToString(item);
                if (!string.IsNullOrEmpty(ip) &&
                    !set.Exists(x => string.Equals(x, ip, StringComparison.OrdinalIgnoreCase)))
                    set.Add(ip);
            }
            return set.ToArray();
        }

        bool TryTargets(out SshKeyInfo key, out string user, out int port, out string[] ips, bool needPriv)
        {
            key = SelectedKey();
            user = "";
            port = 22;
            ips = new string[0];
            if (key == null)
            {
                MessageBox.Show(this, L.T("pickKeyTable"), L.T("titleServers"));
                return false;
            }
            if (needPriv && !key.HasPriv)
            {
                MessageBox.Show(this, L.F("noPriv", key.PrivPath), L.T("titleServers"));
                return false;
            }
            if (string.IsNullOrEmpty(sshExe))
            {
                MessageBox.Show(this, L.T("noSsh"), L.T("titleServers"));
                return false;
            }
            user = string.IsNullOrWhiteSpace(txtUser.Text) ? "root" : txtUser.Text.Trim();
            if (!IsValidUser(user))
            {
                MessageBox.Show(this, L.F("badUser", user), L.T("grpServer"));
                return false;
            }
            port = (int)numPort.Value;
            ips = GetTargetIps();
            if (ips.Length == 0)
            {
                MessageBox.Show(this, L.T("pickServer"), L.T("grpServer"));
                return false;
            }
            var bad = ips.Where(ip => !IsValidHost(ip)).ToArray();
            if (bad.Length > 0)
            {
                MessageBox.Show(this, L.F("badHost", string.Join("\n", bad)), L.T("grpServer"));
                return false;
            }
            SaveSettings();
            return true;
        }

        static string SshPrefix(SshKeyInfo k, string user, string host, int port)
        {
            var id = k != null && k.HasPriv ? "-i " + QuoteArg(k.PrivPath) + " " : "";
            return "-p " + port + " -o StrictHostKeyChecking=accept-new -o ConnectTimeout=10 " + id +
                   "-l " + QuoteArg(user) + " -- " + QuoteArg(host) + " ";
        }

        static string QuoteArg(string s)
        {
            if (s == null) s = "";
            var sb = new StringBuilder("\"");
            for (var i = 0; i < s.Length; i++)
            {
                var bs = 0;
                while (i < s.Length && s[i] == '\\')
                {
                    bs++;
                    i++;
                }
                if (i == s.Length)
                {
                    sb.Append('\\', bs * 2);
                    break;
                }
                if (s[i] == '"')
                {
                    sb.Append('\\', bs * 2 + 1);
                    sb.Append('"');
                }
                else
                {
                    sb.Append('\\', bs);
                    sb.Append(s[i]);
                }
            }
            sb.Append('"');
            return sb.ToString();
        }

        static string Quote(string s)
        {
            return QuoteArg(s);
        }

        static string ShWrap(string cmd)
        {
            return "sh -c '" + cmd.Replace("'", "'\\''") + "'";
        }

        static string PubToB64(string pubLine)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(pubLine + "\n"));
        }

        static bool IsShellSafeForCopy(string s)
        {
            return s.IndexOfAny(new[] { '$', '%', '`', '"', '\'', '\r', '\n',
                '\u2018', '\u2019', '\u201A', '\u201B', '\u201C', '\u201D', '\u201E' }) < 0;
        }

        static string LoginCommand(SshKeyInfo k, string user, string ip, int port)
        {
            return "ssh -i \"" + k.PrivPath + "\" -o IdentitiesOnly=yes -p " + port + " -l \"" + user + "\" " + ip;
        }

        void CopyLoginCommand()
        {
            SshKeyInfo k;
            string user;
            int port;
            string[] ips;
            if (!TryTargets(out k, out user, out port, out ips, true)) return;
            if (!IsShellSafeForCopy(k.PrivPath))
            {
                MessageBox.Show(this, L.F("unsafePath", k.PrivPath), L.T("btnCmd"));
                return;
            }

            var lines = new List<string>();
            foreach (var ip in ips)
                lines.Add(LoginCommand(k, user, ip, port));

            if (!TrySetClipboard(string.Join(Environment.NewLine, lines))) return;
            Log(L.F("loginCmdCopied", KeyLabel(k)), Color.DarkBlue);
            foreach (var line in lines)
                Log(line, Color.Black);
        }

        Action Upload()
        {
            SshKeyInfo k;
            string user;
            int port;
            string[] ips;
            if (!TryTargets(out k, out user, out port, out ips, true)) return null;
            var pubLine = ReadPubLine(k);
            if (pubLine.Length == 0)
            {
                MessageBox.Show(this, L.F("pubReadFail", k.PubPath), L.T("titleServers"));
                return null;
            }
            var blob = PubBlob(pubLine);
            if (blob == null)
            {
                MessageBox.Show(this, L.F("pubBadFormat", k.PubPath), L.T("titleServers"));
                return null;
            }

            const string ak = "~/.ssh/authorized_keys";
            var remote = ShWrap(
                "mkdir -p ~/.ssh && chmod 700 ~/.ssh && touch " + ak + " && chmod 600 " + ak +
                " && K=$(echo " + PubToB64(pubLine) + " | base64 -d | tr -d '\\r') && test -n \"$K\"" +
                " && if [ -s " + ak + " ] && [ -n \"$(tail -c1 " + ak + ")\" ]; then echo >> " + ak + "; fi" +
                " && { awk -v b=" + blob + " '" + AwkFindBlob + "' " + ak + "; c=$?; if [ $c -eq 1 ]; then printf '%s\\n' \"$K\" >> " + ak + "; else [ $c -eq 0 ]; fi; }" +
                " && echo KEY_OK");

            return delegate
            {
                var okN = 0;
                var failN = 0;
                Log(L.F("adding", KeyLabel(k)), Color.DarkBlue);
                Log(L.T("oneSession"), Color.DarkBlue);

                foreach (var ip in ips)
                {
                    Log(L.F("sepUpload", k.Name, user + "@" + ip + ":" + port), Color.Black);
                    if (RunInteractive(sshExe, SshPrefix(k, user, ip, port) + Quote(remote)) != 0)
                    {
                        Log("[FAIL] " + L.F("uploadFail", ip, k.Name), Color.Red);
                        failN++;
                        continue;
                    }
                    if (!CheckLogin(k, user, ip, port))
                    {
                        failN++;
                        continue;
                    }
                    okN++;
                }
                Log(L.F("uploadSummary", k.Name, okN, failN, ips.Length), Color.Black);
            };
        }

        Action CheckOnly()
        {
            SshKeyInfo k;
            string user;
            int port;
            string[] ips;
            if (!TryTargets(out k, out user, out port, out ips, true)) return null;

            return delegate
            {
                var okN = 0;
                var failN = 0;
                Log(L.F("checking", KeyLabel(k)), Color.DarkBlue);
                foreach (var ip in ips)
                {
                    Log(L.F("sepCheck", k.Name, user + "@" + ip + ":" + port), Color.Black);
                    if (CheckLogin(k, user, ip, port)) okN++;
                    else failN++;
                }
                Log(L.F("checkSummary", k.Name, okN, failN, ips.Length), Color.Black);
            };
        }

        Action Revoke()
        {
            SshKeyInfo k;
            string user;
            int port;
            string[] ips;
            if (!TryTargets(out k, out user, out port, out ips, false)) return null;
            var pubLine = ReadPubLine(k);
            if (pubLine.Length == 0)
            {
                MessageBox.Show(this, L.F("pubReadFail", k.PubPath), L.T("btnRevoke"));
                return null;
            }
            var blob = PubBlob(pubLine);
            if (blob == null)
            {
                MessageBox.Show(this, L.F("pubBadFormat", k.PubPath), L.T("btnRevoke"));
                return null;
            }
            var targets = string.Join("\n", ips.Select(ip => "  " + user + "@" + ip + ":" + port));
            if (MessageBox.Show(this, L.F("revokeConfirm", k.Name, targets, KeyLabel(k)), L.T("btnRevoke"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return null;

            const string ak = "~/.ssh/authorized_keys";
            var remote = ShWrap(
                "B=" + blob + " && test -n \"$B\"" +
                " && mkdir -p ~/.ssh && touch " + ak +
                " && T=$(mktemp ~/.ssh/.skm.XXXXXX)" +
                " && { awk -v b=\"$B\" '" + AwkDropBlob + "' " + ak + " > \"$T\" && chmod 600 \"$T\"" +
                " && if [ -L " + ak + " ]; then { cat \"$T\" > " + ak + " || { echo \"SKM: failed to write authorized_keys, filtered copy kept in $T\" >&2; exit 1; }; } && rm -f \"$T\"; else mv \"$T\" " + ak + "; fi" +
                " || { rm -f \"$T\"; false; }; }" +
                " && if awk -v b=\"$B\" '" + AwkFindBlob + "' " + ak + "; then echo KEY_STILL_THERE; exit 1; fi && echo KEY_REMOVED");

            return delegate
            {
                var okN = 0;
                var failN = 0;
                Log(L.F("removing", KeyLabel(k)), Color.DarkBlue);
                foreach (var ip in ips)
                {
                    Log(L.F("sepRevoke", k.Name, user + "@" + ip + ":" + port), Color.Black);
                    if (RunInteractive(sshExe, SshPrefix(k, user, ip, port) + Quote(remote)) != 0)
                    {
                        Log("[FAIL] " + L.F("revokeFail", ip, k.Name), Color.Red);
                        failN++;
                        continue;
                    }
                    if (!k.HasPriv)
                    {
                        Log("[OK] " + L.F("revokedNoPriv", ip, k.Name), Color.DarkOrange);
                        okN++;
                        continue;
                    }
                    string reason;
                    var check = ProbeLogin(k, user, ip, port, out reason);
                    if (check == LoginCheck.Works)
                    {
                        Log("[FAIL] " + L.F("stillWorks", ip, k.Name), Color.Red);
                        failN++;
                        continue;
                    }
                    if (check == LoginCheck.AcceptedNeedsPassphrase)
                    {
                        Log("[FAIL] " + L.F("stillAccepted", ip, k.Name), Color.Red);
                        failN++;
                        continue;
                    }
                    if (check == LoginCheck.Unknown)
                    {
                        Log("[OK] " + L.F("revokedUnknown", ip, k.Name, reason), Color.DarkOrange);
                        okN++;
                        continue;
                    }
                    Log("[OK] " + L.F("revokedOk", ip, k.Name), Color.DarkGreen);
                    okN++;
                }
                Log(L.F("revokeSummary", k.Name, okN, failN, ips.Length), Color.Black);
            };
        }

        static string CheckArgs(SshKeyInfo k, string user, string host, int port)
        {
            return "-v -i " + QuoteArg(k.PrivPath) + " -p " + port +
                   " -o BatchMode=yes -o IdentitiesOnly=yes -o PreferredAuthentications=publickey -o PasswordAuthentication=no -o StrictHostKeyChecking=accept-new -o ConnectTimeout=10" +
                   " -l " + QuoteArg(user) + " -- " + QuoteArg(host) + " " + QuoteArg("echo KEY_LOGIN_OK");
        }

        LoginCheck ProbeLogin(SshKeyInfo k, string user, string host, int port, out string reason)
        {
            var r = RunCaptured(sshExe, CheckArgs(k, user, host, port), CheckTimeoutMs);
            LogStdErr(r.StdErr);

            var fp = FingerprintToken(k.Finger);
            if (fp != null)
            {
                var oursAccepted = false;
                var authed = false;
                var authedOurs = false;
                foreach (var line in r.StdErr.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.IndexOf("Server accepts key", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (IsOurAcceptLine(line, fp, k.PrivPath)) oursAccepted = true;
                    }
                    else if (!authed &&
                             (line.IndexOf("Authenticated to", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              line.IndexOf("Authentication succeeded", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        authed = true;
                        authedOurs = oursAccepted;
                    }
                }
                if (authed || r.ExitCode == 0)
                {
                    var ours = authed
                        ? authedOurs
                        : (oursAccepted || r.StdErr.IndexOf("Server accepts key", StringComparison.OrdinalIgnoreCase) < 0);
                    if (ours)
                    {
                        reason = "";
                        return LoginCheck.Works;
                    }
                    reason = L.T("reasonOtherKey");
                    return LoginCheck.Unknown;
                }
                if (r.TimedOut)
                {
                    reason = L.T("reasonTimeout");
                    return LoginCheck.Unknown;
                }
                if (oursAccepted)
                {
                    reason = "";
                    return LoginCheck.AcceptedNeedsPassphrase;
                }
                if (r.StdErr.IndexOf("Permission denied", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    reason = "";
                    return LoginCheck.Denied;
                }
                reason = L.F("reasonExit", r.ExitCode);
                return LoginCheck.Unknown;
            }

            if (r.ExitCode == 0 ||
                r.StdErr.IndexOf("Authenticated to", StringComparison.OrdinalIgnoreCase) >= 0 ||
                r.StdErr.IndexOf("Authentication succeeded", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                reason = "";
                return LoginCheck.Works;
            }
            if (r.TimedOut)
            {
                reason = L.T("reasonTimeout");
                return LoginCheck.Unknown;
            }
            if (r.StdErr.IndexOf("Server accepts key", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                reason = "";
                return LoginCheck.AcceptedNeedsPassphrase;
            }
            if (r.StdErr.IndexOf("Permission denied", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                reason = "";
                return LoginCheck.Denied;
            }
            reason = L.F("reasonExit", r.ExitCode);
            return LoginCheck.Unknown;
        }

        static string FingerprintToken(string finger)
        {
            if (string.IsNullOrEmpty(finger)) return null;
            foreach (var part in finger.Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
                if (part.StartsWith("SHA256:", StringComparison.Ordinal) && part.Length > 7) return part;
            return null;
        }

        static bool IsOurAcceptLine(string line, string fp, string privPath)
        {
            if (line.IndexOf(fp, StringComparison.Ordinal) >= 0) return true;
            var norm = line.Replace('\\', '/');
            var path = (privPath ?? "").Replace('\\', '/');
            if (path.Length > 0 && norm.IndexOf(path, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            var hasFp = line.IndexOf("SHA256:", StringComparison.Ordinal) >= 0 || line.IndexOf("MD5:", StringComparison.Ordinal) >= 0;
            var hasPath = line.IndexOf('/') >= 0 || line.IndexOf('\\') >= 0;
            return !hasFp && !hasPath;
        }

        bool CheckLogin(SshKeyInfo k, string user, string host, int port)
        {
            Log(L.F("checkingLogin", k.Name, host), Color.DarkBlue);
            string reason;
            var check = ProbeLogin(k, user, host, port, out reason);
            if (check == LoginCheck.Works)
            {
                Log("[OK] " + L.F("loginOk", host, k.Name), Color.DarkGreen);
                return true;
            }
            if (check == LoginCheck.AcceptedNeedsPassphrase)
            {
                Log("[OK] " + L.F("loginNeedsPass", host, k.Name), Color.DarkGreen);
                return true;
            }
            if (check == LoginCheck.Denied)
                Log("[FAIL] " + L.F("loginDenied", host, k.Name), Color.Red);
            else
                Log("[FAIL] " + L.F("loginUnknown", host, k.Name, reason), Color.Red);
            return false;
        }

        bool TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                if (!IsFileError(ex)) throw;
                Log("[FAIL] " + L.F("deleteFail", path, ex.Message), Color.Red);
            }
            return false;
        }

        static void TryDeleteQuiet(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception ex)
            {
                if (!IsFileError(ex)) throw;
            }
        }

        static int RunInteractive(string file, string args)
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = file;
                p.StartInfo.Arguments = args;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = false;
                p.Start();
                p.WaitForExit();
                return p.ExitCode;
            }
        }

        static ProcResult RunCaptured(string file, string args, int timeoutMs)
        {
            var res = new ProcResult();
            var outSb = new StringBuilder();
            var errSb = new StringBuilder();
            using (var p = new Process())
            {
                p.StartInfo.FileName = file;
                p.StartInfo.Arguments = args;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                p.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                p.OutputDataReceived += delegate(object s, DataReceivedEventArgs e)
                {
                    if (e.Data != null) lock (outSb) outSb.AppendLine(e.Data);
                };
                p.ErrorDataReceived += delegate(object s, DataReceivedEventArgs e)
                {
                    if (e.Data != null) lock (errSb) errSb.AppendLine(e.Data);
                };
                p.Start();
                try { p.StandardInput.Close(); }
                catch (IOException) { }
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                if (p.WaitForExit(timeoutMs))
                {
                    p.WaitForExit();
                    res.ExitCode = p.ExitCode;
                }
                else
                {
                    res.TimedOut = true;
                    try { p.Kill(); }
                    catch (InvalidOperationException) { }
                    catch (Win32Exception) { }
                    p.WaitForExit(5000);
                }
            }
            lock (outSb) res.StdOut = outSb.ToString();
            lock (errSb) res.StdErr = errSb.ToString();
            return res;
        }
    }
}
