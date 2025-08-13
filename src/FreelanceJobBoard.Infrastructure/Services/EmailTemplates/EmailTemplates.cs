namespace FreelanceJobBoard.Infrastructure.Services.EmailTemplates;

public static class EmailTemplates
{
    public static string JobStatusUpdateTemplate => @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Job Status Update</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #007bff; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background: #f9f9f9; }
        .status-badge { 
            display: inline-block; 
            padding: 8px 16px; 
            border-radius: 4px; 
            font-weight: bold; 
            text-transform: uppercase;
        }
        .accepted { background: #28a745; color: white; }
        .rejected { background: #dc3545; color: white; }
        .pending { background: #ffc107; color: #212529; }
        .footer { text-align: center; padding: 20px; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Job Application Update</h1>
        </div>
        <div class='content'>
            <h2>Hello {{FreelancerName}},</h2>
            <p>We have an update regarding your application for the job:</p>
            <h3>{{JobTitle}}</h3>
            <p>Status: <span class='status-badge {{StatusClass}}'>{{Status}}</span></p>
            {{#if ClientMessage}}
            <div style='background: white; padding: 15px; border-left: 4px solid #007bff; margin: 20px 0;'>
                <h4>Message from Client:</h4>
                <p>{{ClientMessage}}</p>
            </div>
            {{/if}}
            <p>You can view more details by logging into your account.</p>
            <p>Best regards,<br>FreelanceJobBoard Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 FreelanceJobBoard. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

    public static string NewProposalTemplate => @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>New Proposal Received</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #28a745; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background: #f9f9f9; }
        .proposal-details { background: white; padding: 15px; border-radius: 5px; margin: 15px 0; }
        .footer { text-align: center; padding: 20px; font-size: 12px; color: #666; }
        .btn { 
            display: inline-block; 
            padding: 12px 24px; 
            background: #007bff; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px; 
            margin: 10px 0;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>New Proposal Received!</h1>
        </div>
        <div class='content'>
            <h2>Hello {{ClientName}},</h2>
            <p>Great news! You've received a new proposal for your job:</p>
            <h3>{{JobTitle}}</h3>
            <div class='proposal-details'>
                <h4>Proposal Details:</h4>
                <p><strong>Freelancer:</strong> {{FreelancerName}}</p>
                <p><strong>Bid Amount:</strong> ${{BidAmount}}</p>
                <p><strong>Estimated Timeline:</strong> {{Timeline}} days</p>
            </div>
            <p>Review the full proposal and freelancer profile in your dashboard.</p>
            <a href='#' class='btn'>View Proposal</a>
            <p>Best regards,<br>FreelanceJobBoard Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 FreelanceJobBoard. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

    public static string JobApprovalTemplate => @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Job Approval Status</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { padding: 20px; text-align: center; }
        .approved { background: #28a745; color: white; }
        .rejected { background: #dc3545; color: white; }
        .content { padding: 20px; background: #f9f9f9; }
        .footer { text-align: center; padding: 20px; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header {{StatusClass}}'>
            <h1>Job {{#if IsApproved}}Approved{{else}}Rejected{{/if}}</h1>
        </div>
        <div class='content'>
            <h2>Hello {{ClientName}},</h2>
            <p>Your job posting has been {{#if IsApproved}}approved{{else}}rejected{{/if}} by our admin team:</p>
            <h3>{{JobTitle}}</h3>
            {{#if AdminMessage}}
            <div style='background: white; padding: 15px; border-left: 4px solid #007bff; margin: 20px 0;'>
                <h4>Admin Message:</h4>
                <p>{{AdminMessage}}</p>
            </div>
            {{/if}}
            {{#if IsApproved}}
            <p>Your job is now live and visible to freelancers. You can expect to start receiving proposals soon!</p>
            {{else}}
            <p>Please review the feedback and make necessary changes before resubmitting.</p>
            {{/if}}
            <p>Best regards,<br>FreelanceJobBoard Admin Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 FreelanceJobBoard. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

    public static string ReviewNotificationTemplate => @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>New Review Received</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #ff9500; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background: #f9f9f9; }
        .review-details { background: white; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ff9500; }
        .rating { font-size: 24px; color: #ff9500; margin: 10px 0; }
        .stars { color: #ffd700; }
        .footer { text-align: center; padding: 20px; font-size: 12px; color: #666; }
        .btn { 
            display: inline-block; 
            padding: 12px 24px; 
            background: #007bff; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px; 
            margin: 10px 0;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>You've Received a New Review!</h1>
        </div>
        <div class='content'>
            <h2>Hello {{RevieweeName}},</h2>
            <p>Great news! You've received a new review for the completed job:</p>
            <h3>{{JobTitle}}</h3>
            <div class='review-details'>
                <h4>Review Details:</h4>
                <div class='rating'>
                    <span class='stars'>{{StarRating}}</span> ({{Rating}}/5)
                </div>
                <p><strong>From:</strong> {{ReviewerName}}</p>
                <p><strong>Comment:</strong></p>
                <p style='font-style: italic; background: #f8f9fa; padding: 10px; border-radius: 3px;'>{{Comment}}</p>
            </div>
            <p>This review will help build your reputation on our platform and attract more clients!</p>
            <a href='#' class='btn'>View Your Profile</a>
            <p>Best regards,<br>FreelanceJobBoard Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 FreelanceJobBoard. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

    public static string JobCompletedClientNotificationTemplate => @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Job Completed - Time to Review!</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #28a745; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background: #f9f9f9; }
        .completion-details { background: white; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #28a745; }
        .footer { text-align: center; padding: 20px; font-size: 12px; color: #666; }
        .btn { 
            display: inline-block; 
            padding: 12px 24px; 
            background: #ff9500; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px; 
            margin: 10px 5px;
            text-decoration: none;
        }
        .btn-secondary { 
            background: #6c757d; 
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? Job Completed Successfully!</h1>
        </div>
        <div class='content'>
            <h2>Hello {{ClientName}},</h2>
            <p>Great news! Your freelancer has marked the following job as completed:</p>
            <div class='completion-details'>
                <h3>{{JobTitle}}</h3>
                <p><strong>Completed by:</strong> {{FreelancerName}}</p>
                <p><strong>Completion Date:</strong> {{CompletionDate}}</p>
            </div>
            <p>Now it's time to review the work and share your experience with {{FreelancerName}}. Your review helps build our community and assists other clients in finding quality freelancers.</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{{ReviewUrl}}' class='btn'>Leave a Review</a>
                <a href='/jobs/{{JobId}}' class='btn btn-secondary'>View Job Details</a>
            </div>
            <p><strong>Why Reviews Matter:</strong></p>
            <ul>
                <li>Help other clients find quality freelancers</li>
                <li>Provide valuable feedback to improve services</li>
                <li>Build a trustworthy community</li>
                <li>Enhance the freelancer's reputation</li>
            </ul>
            <p>Thank you for using FreelanceJobBoard!</p>
            <p>Best regards,<br>FreelanceJobBoard Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 FreelanceJobBoard. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

    public static string JobCompletedFreelancerNotificationTemplate => @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Job Completed - Review Your Client!</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #17a2b8; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background: #f9f9f9; }
        .completion-details { background: white; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #17a2b8; }
        .footer { text-align: center; padding: 20px; font-size: 12px; color: #666; }
        .btn { 
            display: inline-block; 
            padding: 12px 24px; 
            background: #ff9500; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px; 
            margin: 10px 5px;
        }
        .btn-secondary { 
            background: #6c757d; 
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? Congratulations on Completing Your Job!</h1>
        </div>
        <div class='content'>
            <h2>Hello {{FreelancerName}},</h2>
            <p>Excellent work! You've successfully completed:</p>
            <div class='completion-details'>
                <h3>{{JobTitle}}</h3>
                <p><strong>Client:</strong> {{ClientName}}</p>
                <p><strong>Completion Date:</strong> {{CompletionDate}}</p>
            </div>
            <p>Now it's time to share your experience working with {{ClientName}}. Your review helps other freelancers understand what it's like to work with this client.</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{{ReviewUrl}}' class='btn'>Review Your Client</a>
                <a href='/jobs/{{JobId}}' class='btn btn-secondary'>View Job Details</a>
            </div>
            <p><strong>Benefits of Reviewing Clients:</strong></p>
            <ul>
                <li>Help other freelancers make informed decisions</li>
                <li>Provide feedback to improve client experience</li>
                <li>Build a transparent and trustworthy platform</li>
                <li>Share insights about working relationships</li>
            </ul>
            <p>Keep up the great work, and we look forward to seeing more successful projects from you!</p>
            <p>Best regards,<br>FreelanceJobBoard Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 FreelanceJobBoard. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

    public static string ReviewReminderTemplate => @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Don't Forget to Leave a Review!</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #ffc107; color: #212529; padding: 20px; text-align: center; }
        .content { padding: 20px; background: #f9f9f9; }
        .job-details { background: white; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107; }
        .footer { text-align: center; padding: 20px; font-size: 12px; color: #666; }
        .btn { 
            display: inline-block; 
            padding: 12px 24px; 
            background: #ff9500; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px; 
            margin: 10px 0;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? Review Reminder</h1>
        </div>
        <div class='content'>
            <h2>Hello {{UserName}},</h2>
            <p>We hope you had a great experience with your recently completed job! We noticed you haven't left a review yet.</p>
            <div class='job-details'>
                <h3>{{JobTitle}}</h3>
                <p><strong>{{ReviewType}}:</strong> {{OtherPartyName}}</p>
                <p><strong>Completed:</strong> {{CompletionDate}}</p>
            </div>
            <p>Your review is valuable to our community and helps maintain quality standards. It only takes a few minutes!</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{{ReviewUrl}}' class='btn'>Leave Your Review Now</a>
            </div>
            <p><em>This is a friendly reminder. Reviews help build trust and improve our platform for everyone.</em></p>
            <p>Best regards,<br>FreelanceJobBoard Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 FreelanceJobBoard. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
}