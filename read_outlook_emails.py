import win32com.client
import sys

def fetch_recent_emails(limit=5):
    try:
        outlook = win32com.client.Dispatch("Outlook.Application").GetNamespace("MAPI")
    except Exception as e:
        print(f"Error connecting to Outlook: {e}")
        return

    print("=== Outlook Accounts / Stores Found ===")
    target_folder = None
    for folder in outlook.Folders:
        print(f"- Store: {folder.Name}")
        if "docentrapdf" in folder.Name.lower():
            target_folder = folder

    # If support@docentrapdf.com is a separate store, find its Inbox
    inbox = None
    if target_folder:
        try:
            for sub in target_folder.Folders:
                if sub.Name.lower() in ["gelen kutusu", "inbox"]:
                    inbox = sub
                    break
        except Exception as e:
            print(f"Error checking subfolders: {e}")

    # Fallback to default inbox
    if not inbox:
        try:
            inbox = outlook.GetDefaultFolder(6) # 6 = olFolderInbox
        except Exception as e:
            print(f"Error accessing default inbox: {e}")
            return

    print(f"\nTarget Inbox: {inbox.FolderPath} (Total Items: {inbox.Items.Count})\n")
    
    messages = inbox.Items
    messages.Sort("[ReceivedTime]", True)
    
    count = 0
    for msg in messages:
        if count >= limit:
            break
        try:
            subject = getattr(msg, "Subject", "No Subject")
            sender = getattr(msg, "SenderName", "Unknown Sender")
            sender_email = getattr(msg, "SenderEmailAddress", "")
            received = getattr(msg, "ReceivedTime", "")
            body = getattr(msg, "Body", "")
            # Short preview
            body_preview = " ".join(body.split())[:300] if body else "(Empty body)"
            
            print(f"[{count+1}] Subject: {subject}")
            print(f"    From: {sender} <{sender_email}>")
            print(f"    Date: {received}")
            print(f"    Preview: {body_preview}")
            print("-" * 50)
            count += 1
        except Exception as e:
            continue

if __name__ == "__main__":
    fetch_recent_emails(5)
