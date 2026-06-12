interface Props {
  lastPushStatus: string | null;
}

/** Parses a log line of the form "YYYY-MM-DD HH:MM:SS | PUSH | message" */
function parseStatusLine(line: string): { timestamp: string; message: string } | null {
  const parts = line.split(' | ');
  if (parts.length < 3) return null;
  return { timestamp: parts[0].trim(), message: parts.slice(2).join(' | ').trim() };
}

export default function StatusBar({ lastPushStatus }: Props) {
  const parsed = lastPushStatus ? parseStatusLine(lastPushStatus) : null;

  return (
    <div className="status-bar">
      {parsed ? (
        <span>
          <span className="status-label">Last push:</span>{' '}
          <span className="status-timestamp">{parsed.timestamp}</span>
          {' — '}
          <span className="status-message">{parsed.message}</span>
        </span>
      ) : (
        <span className="status-empty">No pushes recorded yet.</span>
      )}
    </div>
  );
}
