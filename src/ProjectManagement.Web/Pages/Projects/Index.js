$(function () {
    var l = abp.localization.getResource('ProjectManagement');
    var $searchInput = $('#ProjectSearch');
    var $searchButton = $('#ProjectSearchButton');
    var $searchResults = $('#ProjectSearchResults');
    var suggestionRequestToken = 0;

    function runSearch() {
        $searchResults.addClass('d-none').empty();
        dataTable.ajax.reload();
    }

    function hideSuggestions() {
        $searchResults.addClass('d-none').empty();
    }

    function renderSuggestions(items) {
        $searchResults.empty();

        if (!items || !items.length) {
            $searchResults
                .append(
                    $('<div/>', {
                        class: 'project-search-empty',
                        text: l('NoDataAvailable'),
                    }),
                )
                .removeClass('d-none');
            return;
        }

        items.forEach(function (item) {
            $('<button/>', {
                type: 'button',
                class: 'project-search-item',
                text: item.name || '-',
            })
                .on('click', function () {
                    $searchInput.val(item.name || '');
                    hideSuggestions();
                    $searchInput.trigger('focus');
                })
                .appendTo($searchResults);
        });

        $searchResults.removeClass('d-none');
    }

    var fetchSuggestions = abp.utils.debounce(function () {
        var keyword = ($searchInput.val() || '').trim();

        if (!keyword) {
            hideSuggestions();
            return;
        }

        var currentToken = ++suggestionRequestToken;
        projectManagement.projects.project
            .getList({
                filter: keyword,
                skipCount: 0,
                maxResultCount: 6,
                sorting: 'name',
            })
            .then(function (response) {
                if (currentToken !== suggestionRequestToken) {
                    return;
                }

                var items = response && response.items ? response.items : [];
                renderSuggestions(items);
            })
            .catch(function () {
                if (currentToken !== suggestionRequestToken) {
                    return;
                }

                hideSuggestions();
            });
    }, 250);

    var dataTable = $('#ProjectTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[0, 'asc']],
            searching: false,
            scrollX: true,
            processing: true,
            ajax: abp.libs.datatables.createAjax(
                projectManagement.projects.project.getList,
                function () {
                    return {
                        filter: $searchInput.val(),
                    };
                },
            ),
            columnDefs: [
                {
                    title: l('Actions'),
                    width: '20%',
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted(
                                    'ProjectManagement.Projects.Edit',
                                ),
                                action: function (data) {
                                    editProject(data.record.id);
                                },
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted(
                                    'ProjectManagement.Projects.Delete',
                                ),
                                confirmMessage: function (data) {
                                    return l(
                                        'ProjectsDeleteConfirmationMessage',
                                    );
                                },
                                action: function (data) {
                                    projectManagement.projects.project
                                        .delete(data.record.id)
                                        .then(function () {
                                            abp.notify.success(
                                                l('DeletedSuccessfully'),
                                            );
                                            dataTable.ajax.reload();
                                        });
                                },
                            },
                        ],
                    },
                },
                {
                    title: l('Color'),
                    data: 'color',
                    width: '10%',
                    render: function (data) {
                        return (
                            '<span style="display: inline-block; width: 15px; height: 15px; background-color: ' +
                            (data || '#808080') +
                            '; border-radius: 10px; border: 1px solid #ddd;"></span> '
                        );
                    },
                    defaultContent: '',
                },
                {
                    title: l('Name'),
                    data: 'name',
                    width: '20%',
                    defaultContent: '',
                },
                {
                    title: l('Description'),
                    data: 'description',
                    width: '40%',
                    render: function (data) {
                        return data || '-';
                    },
                    defaultContent: '',
                },
            ],
        }),
    );

    // Store table reference globally so modals can access it
    window.projectTable = dataTable;

    var createModal = new abp.ModalManager(
        abp.appPath + 'Projects/CreateModal',
    );
    var editModal = new abp.ModalManager(abp.appPath + 'Projects/EditModal');

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    // Listen sự kiện project được tạo từ CreateModal
    document.addEventListener('projectCreated', function () {
        dataTable.ajax.reload();
    });

    $searchInput.on('input', function () {
        fetchSuggestions();
    });

    $searchInput.on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            runSearch();
        }
    });

    $searchButton.on('click', function () {
        runSearch();
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('.project-search-wrap').length) {
            hideSuggestions();
        }
    });

    $('#NewProjectButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    function editProject(id) {
        editModal.open({ id: id });
    }
});
