import { useEffect, useRef, useState } from 'react';
import { invoke } from '@tauri-apps/api/core';
import type { MeInfo, Session } from '../types';

interface Props {
  session: Session;
  isDirty: boolean;
  onSaveAndSignOut: () => Promise<void>;
  onSignOut: () => Promise<void>;
}

type SignOutDialog = 'clean' | 'dirty' | null;

export default function AccountMenu({ session, isDirty, onSaveAndSignOut, onSignOut }: Props) {
  const [open_menu, setOpenMenu]             = useState(false);
  const [signOutDialog, setSignOutDialog]    = useState<SignOutDialog>(null);
  const [showMyAccount, setShowMyAccount]    = useState(false);
  const [meInfo, setMeInfo]                  = useState<MeInfo | null>(null);
  const [meLoading, setMeLoading]            = useState(false);
  const [meError, setMeError]                = useState<string | null>(null);
  const menuRef = useRef<HTMLDivElement>(null);

  // Close menu on outside click
  useEffect(() => {
    if (!open_menu) return;
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setOpenMenu(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [open_menu]);

  const handleMyAccount = async () => {
    setOpenMenu(false);
    setShowMyAccount(true);
    setMeError(null);
    setMeLoading(true);
    try {
      const info = await invoke<MeInfo>('auth_get_me');
      setMeInfo(info);
    } catch (err) {
      setMeError(String(err));
    } finally {
      setMeLoading(false);
    }
  };

  const handleSignOutClick = () => {
    setOpenMenu(false);
    setSignOutDialog(isDirty ? 'dirty' : 'clean');
  };

  const handleConfirmSignOut = async () => {
    setSignOutDialog(null);
    await onSignOut();
  };

  const handleSaveAndSignOut = async () => {
    setSignOutDialog(null);
    await onSaveAndSignOut();
  };

  const openDashboard = () => {
    invoke('open_url', { url: 'https://wowwtfsync.com/dashboard.html' }).catch(() => {});
  };

  const subStatusLabel = (status: string | null | undefined) => {
    if (!status || status === 'inactive') return { text: 'Inactive', cls: 'account-sub-inactive' };
    if (status === 'active') return { text: 'Active', cls: 'account-sub-active' };
    if (status === 'cancelled') return { text: 'Cancelled', cls: 'account-sub-inactive' };
    return { text: status, cls: '' };
  };

  return (
    <>
      {/* ── Menu bar ─────────────────────────────────────────── */}
      <div className="menu-bar">
        <div className="menu-bar-item" ref={menuRef}>
          <button
            className={`menu-bar-btn${open_menu ? ' menu-bar-btn-active' : ''}`}
            onClick={() => setOpenMenu(v => !v)}
          >
            Account ({session.user.username})
          </button>

          {open_menu && (
            <div className="menu-dropdown">
              <button className="menu-dropdown-item" onClick={handleMyAccount}>
                My Account
              </button>
              <div className="menu-dropdown-sep" />
              <button className="menu-dropdown-item menu-dropdown-item-danger" onClick={handleSignOutClick}>
                Sign Out
              </button>
            </div>
          )}
        </div>
      </div>

      {/* ── My Account dialog ────────────────────────────────── */}
      {showMyAccount && (
        <div className="dialog-backdrop">
          <div className="dialog account-dialog">
            <div className="account-dialog-header">My Account</div>

            {meLoading && (
              <p className="dialog-message account-loading">Loading…</p>
            )}

            {meError && (
              <p className="dialog-message account-error">{meError}</p>
            )}

            {meInfo && !meLoading && (
              <div className="account-info">
                <div className="account-row">
                  <span className="account-label">Username</span>
                  <span className="account-value">{meInfo.username}</span>
                </div>
                <div className="account-row">
                  <span className="account-label">Email</span>
                  <span className="account-value">{meInfo.email}</span>
                </div>
                <div className="account-row">
                  <span className="account-label">Subscription</span>
                  <span className={`account-value ${subStatusLabel(meInfo.sub_status).cls}`}>
                    {subStatusLabel(meInfo.sub_status).text}
                    {meInfo.cancel_at_period_end ? ' (cancels at period end)' : ''}
                  </span>
                </div>
                <div className="account-row">
                  <span className="account-label">License Key</span>
                  <span className="account-value account-license">
                    {meInfo.license_key ?? '—'}
                  </span>
                </div>
              </div>
            )}

            <div className="dialog-footer account-dialog-footer">
              <button className="btn account-dashboard-btn" onClick={openDashboard}>
                View Dashboard ↗
              </button>
              <button className="btn btn-primary" onClick={() => setShowMyAccount(false)}>
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── Sign Out (no unsaved changes) ────────────────────── */}
      {signOutDialog === 'clean' && (
        <div className="dialog-backdrop">
          <div className="dialog confirm-dialog">
            <p className="dialog-message">Are you sure you want to sign out?</p>
            <div className="dialog-footer">
              <button className="btn btn-danger" onClick={handleConfirmSignOut}>Yes</button>
              <button className="btn" onClick={() => setSignOutDialog(null)}>No</button>
            </div>
          </div>
        </div>
      )}

      {/* ── Sign Out (with unsaved changes) ──────────────────── */}
      {signOutDialog === 'dirty' && (
        <div className="dialog-backdrop">
          <div className="dialog unsaved-dialog">
            <p className="dialog-message">
              You have unsaved changes. What would you like to do?
            </p>
            <div className="dialog-footer unsaved-dialog-footer">
              <button className="btn" onClick={() => setSignOutDialog(null)}>Cancel</button>
              <div className="unsaved-dialog-actions">
                <button className="btn" onClick={handleConfirmSignOut}>
                  Sign Out Without Saving
                </button>
                <button className="btn btn-primary" onClick={handleSaveAndSignOut}>
                  Save and Sign Out
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
