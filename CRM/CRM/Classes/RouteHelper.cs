namespace CRM;

public class RouteHelper
{
    HttpContext? _context;
    private RouteInformation _routeInfo;

    public RouteHelper(HttpContext? httpContext)
    {
        _context = httpContext;
        _routeInfo = new RouteInformation();
        string path = "";

        if (_context != null) {
            try {
                path += _context.Request.Path;
            } catch { }

            if (!String.IsNullOrEmpty(path)) {
                path = path.Replace("\\", "/");
                var parts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts != null && parts.Length > 0) {
                    _routeInfo.Action = parts[0];

                    if (parts.Length > 1) {
                        _routeInfo.Id = parts[1];
                    }

                    if (parts.Length > 2) {
                        _routeInfo.Extra = new List<string>();
                        for (int x = 2; x < parts.Length; x++) {
                            _routeInfo.Extra.Add(parts[x]);
                        }
                    }
                }
            }
        }
    }

    public string Action {
        get {
            string output = String.Empty;
            if (!String.IsNullOrEmpty(_routeInfo.Action)) {
                output = _routeInfo.Action;
            }
            return output;
        }
    }

    public string Id {
        get {
            string output = String.Empty;
            if (!String.IsNullOrEmpty(_routeInfo.Id)) {
                output = _routeInfo.Id;
            }
            return output;
        }
    }

    public List<string>? Extra {
        get {
            return _routeInfo.Extra;
        }
    }

    public string GetBaseUrl()
    {
        string output = "";

        if (_context != null) {
            try {
                output = string.Concat(
                    _context.Request.Scheme,
                    "://",
                    _context.Request.Host.ToUriComponent(),
                    _context.Request.PathBase.ToUriComponent()
                );
                if (!output.EndsWith("/")) {
                    output += "/";
                }
            } catch { }
        }

        return output;
    }

    public RouteInformation RouteInfo {
        get {
            return _routeInfo;
        }
    }

    public class RouteInformation
    {
        public string? Action { get; set; }
        public string? Id { get; set; }
        public List<string>? Extra { get; set; }
    }
}
