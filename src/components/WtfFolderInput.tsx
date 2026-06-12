import { useState } from 'react';
import { open } from '@tauri-apps/plugin-dialog';
import { FolderOpen } from 'lucide-react';

interface Props {
  wtfPath: string;
  onApply: (path: string) => void;
}

export default function WtfFolderInput({ wtfPath, onApply }: Props) {
  const [inputValue, setInputValue] = useState(wtfPath);

  // Keep input in sync when the prop changes (e.g. on initial load).
  if (inputValue !== wtfPath && wtfPath !== '') {
    // Only sync on first load (when local state is still empty).
    if (inputValue === '') setInputValue(wtfPath);
  }

  const handleBrowse = async () => {
    const selected = await open({ directory: true, multiple: false });
    if (typeof selected === 'string' && selected) {
      setInputValue(selected);
    }
  };

  const handleApply = () => {
    if (inputValue.trim()) onApply(inputValue.trim());
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') handleApply();
  };

  return (
    <div className="wtf-folder-row">
      <label className="wtf-label">WTF Folder:</label>
      <input
        className="wtf-input"
        type="text"
        value={inputValue}
        onChange={e => setInputValue(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder="Path to your WoW WTF folder…"
        spellCheck={false}
      />
      <button className="btn btn-icon" onClick={handleBrowse} title="Browse">
        <FolderOpen size={15} />
        <span>Browse</span>
      </button>
      <button
        className="btn btn-primary"
        onClick={handleApply}
        disabled={!inputValue.trim()}
      >
        Apply
      </button>
    </div>
  );
}
