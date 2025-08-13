$(document).ready(function () {
    var jobs = new Bloodhound({
        datumTokenizer: Bloodhound.tokenizers.obj.whitespace('title'),
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        remote: {
            url: '/Jobs/search?query=%QUERY',
            wildcard: '%QUERY'
        }
    });

    $('#Search').typeahead({
        minLength: 2,
        highlight: true
    }, {
        name: 'jobs',
        display: 'title',
        source: jobs,
        templates: {
            empty: '<div class="tt-empty">No jobs found!</div>',
            suggestion: Handlebars.compile(`
            <div class="search-result-item">
                <div class="result-header">
                    <span class="result-title">{{title}}</span>
                    <span class="result-budget">{{ budgetMin }} - {{ budgetMax }}</span>
                </div>
                <div class="result-meta">
                    <span>{{clientName}}</span>
                    <span>Posted {{timeAgo}}</span>
                    <span>{{deadline}}</span>
                </div>
                <div class="result-desc">{{description}}</div>
            </div>
        `)
        }
    }).on('typeahead:select', function (e, job) {
        window.location.href = `/Jobs/PublicJobDetails?jobId=${job.id}`;
    });

});
