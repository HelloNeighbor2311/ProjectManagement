$(function () {
    var l = abp.localization.getResource('ProjectManagement');
    var lastSearchValue = '';

    var dataTable = $('#ProjectTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[0, 'asc']],
            searching: false,
            scrollX: true,
            processing: true,
            // Send current keyword to backend as `filter` on every table reload.
            ajax: abp.libs.datatables.createAjax(
                projectManagement.projects.project.getList,
                function () {
                    return {
                        filter: $('#ProjectSearch').val(),
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

    // Search only when user explicitly clicks the magnifier button.
    $('#ProjectSearchButton').on('click', function () {
        lastSearchValue = ($('#ProjectSearch').val() || '').trim();
        dataTable.ajax.reload();
    });

    // Enter key behaves the same as clicking the magnifier button.
    $('#ProjectSearch').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            lastSearchValue = ($('#ProjectSearch').val() || '').trim();
            dataTable.ajax.reload();
        }
    });

    // Auto reset to unfiltered list when search box is cleared (Backspace or native clear "X").
    $('#ProjectSearch').on('input search', function () {
        var currentValue = ($(this).val() || '').trim();
        if (lastSearchValue !== '' && currentValue === '') {
            dataTable.ajax.reload();
        }
        lastSearchValue = currentValue;
    });

    $('#NewProjectButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    function editProject(id) {
        editModal.open({ id: id });
    }
});
