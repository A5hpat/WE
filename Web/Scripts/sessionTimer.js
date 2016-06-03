var SessionTimer = (function(options) {
	var warningTimer;
	var expiredTimer;

	var sessionTimeoutInMilliseconds = Number(options.sessionTimeoutInMilliseconds) || 0;
	var refreshUrl = options.refreshUrl;
	var warningCallback = options.warningCallback || function() { };
	var expiredCallback = options.expiredCallback || function() { };
	var refreshedCallback = options.refreshedCallback || function() { };

	function start() {
		// Stop the timers
		clearTimeout(expiredTimer);
		clearTimeout(warningTimer);

		// Sanity check
		if(sessionTimeoutInMilliseconds < 60000)
			return;

		// One minute warning
		warningTimer = setTimeout(warningCallback, sessionTimeoutInMilliseconds - 60000);

		// Session ended
		expiredTimer = setTimeout(expiredCallback, sessionTimeoutInMilliseconds);
	}

	function refresh() {
		// Close the warning dialog if it was open
		refreshedCallback();

		// AJAX call to refresh the session
		adnsf$.post(refreshUrl);

		// Start the timers again
		start();
	}

	return {
		start: start,
		refresh: refresh
	};
});